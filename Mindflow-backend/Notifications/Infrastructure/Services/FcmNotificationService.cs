using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Notifications.Application.Services;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Notifications.Infrastructure.Services;

public class FcmNotificationService(
    AppDbContext dbContext,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<FcmNotificationService> logger) : INotificationService
{
    private const string FcmScope = "https://www.googleapis.com/auth/firebase.messaging";

    public async Task SendToAllAsync(string title, string body, CancellationToken ct = default)
    {
        var tokens = await dbContext.DeviceTokens
            .AsNoTracking()
            .Select(dt => dt.Token)
            .ToListAsync(ct);

        await SendBatchAsync(tokens, title, body, ct);
    }

    public async Task SendToUserAsync(int userId, string title, string body, CancellationToken ct = default)
    {
        var tokens = await dbContext.DeviceTokens
            .AsNoTracking()
            .Where(dt => dt.UserId == userId)
            .Select(dt => dt.Token)
            .ToListAsync(ct);

        await SendBatchAsync(tokens, title, body, ct);
    }

    private async Task SendBatchAsync(IEnumerable<string> tokens, string title, string body, CancellationToken ct)
    {
        var accessToken = await GetAccessTokenAsync();
        if (accessToken is null) return;

        var projectId = configuration["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId)) return;

        var client = httpClientFactory.CreateClient("Fcm");
        var url = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";
        var deadTokens = new List<string>();

        foreach (var token in tokens)
        {
            var tokenPreview = token.Length > 10 ? token[..10] : token;
            try
            {
                var payload = new
                {
                    message = new
                    {
                        token,
                        notification = new { title, body }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("FCM send failed for token {Token}: {Status}", tokenPreview, response.StatusCode);
                    if ((int)response.StatusCode is 400 or 404)
                        deadTokens.Add(token);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "FCM send error for token {Token}.", tokenPreview);
            }
        }

        if (deadTokens.Count > 0)
        {
            await dbContext.DeviceTokens
                .Where(dt => deadTokens.Contains(dt.Token))
                .ExecuteDeleteAsync(ct);
            logger.LogInformation("Removed {Count} invalid FCM tokens.", deadTokens.Count);
        }
    }

    private async Task<string?> GetAccessTokenAsync()
    {
        GoogleCredential? credential = null;

        var jsonContent = configuration["Firebase:ServiceAccountJson"];
        if (!string.IsNullOrWhiteSpace(jsonContent))
        {
            try { credential = GoogleCredential.FromJson(jsonContent).CreateScoped(FcmScope); }
            catch (Exception ex) { logger.LogWarning(ex, "Failed to load Firebase credential from JSON env var."); }
        }

        if (credential is null)
        {
            var path = configuration["Firebase:ServiceAccountPath"];
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                logger.LogDebug("Firebase service account not configured — push notifications disabled.");
                return null;
            }
            try { credential = GoogleCredential.FromFile(path).CreateScoped(FcmScope); }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to load Firebase credential from file.");
                return null;
            }
        }

        try { return await ((ITokenAccess)credential).GetAccessTokenForRequestAsync(); }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to obtain Firebase access token.");
            return null;
        }
    }
}
