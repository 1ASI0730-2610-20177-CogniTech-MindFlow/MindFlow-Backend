using Mindflow_backend.Journal.Application.Services;

namespace Mindflow_backend.Journal.Infrastructure.Services;

public class LocalFileStorageService(IWebHostEnvironment env, IHttpContextAccessor http) : IFileStorageService
{
    private static readonly HashSet<string> Allowed =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".mp3", ".wav", ".ogg",
        ".mp4", ".webm",
        ".pdf"
    ];

    public async Task<(string Url, string Type)> SaveAsync(IFormFile file, int userId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext))
            throw new InvalidOperationException($"Tipo de archivo no permitido: {ext}");

        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads", userId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);

        var type = ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
            ".mp3" or ".wav" or ".ogg" => "audio",
            ".mp4" or ".webm" => "video",
            ".pdf" => "document",
            _ => "file"
        };

        var req = http.HttpContext!.Request;
        var scheme = req.Headers.ContainsKey("X-Forwarded-Proto")
            ? req.Headers["X-Forwarded-Proto"].ToString()
            : req.Scheme;
        if (scheme != "https" && !req.Host.Host.Contains("localhost"))
            scheme = "https";
        var url = $"{scheme}://{req.Host}/uploads/{userId}/{fileName}";

        return (url, type);
    }
}
