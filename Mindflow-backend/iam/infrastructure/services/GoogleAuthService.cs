using Google.Apis.Auth;
using Mindflow_backend.iam.application.services;

namespace Mindflow_backend.iam.infrastructure.services;

public class GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger) : IGoogleAuthService
{
    public async Task<GoogleUserInfo?> ValidateAsync(string credential)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [configuration["Google:ClientId"]!]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            return new GoogleUserInfo(payload.Subject, payload.Email, payload.Name);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Google authentication validation failed.");
            return null;
        }
    }
}
