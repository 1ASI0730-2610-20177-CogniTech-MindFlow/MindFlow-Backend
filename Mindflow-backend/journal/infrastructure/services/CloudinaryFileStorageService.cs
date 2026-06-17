using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Mindflow_backend.Journal.Application.Services;

namespace Mindflow_backend.Journal.Infrastructure.Services;

public class CloudinaryFileStorageService(IConfiguration configuration) : IFileStorageService
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

        var type = ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "image",
            ".mp3" or ".wav" or ".ogg" => "audio",
            ".mp4" or ".webm" => "video",
            ".pdf" => "document",
            _ => "file"
        };

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        var account = new Account(cloudName, apiKey, apiSecret);
        var cloudinary = new Cloudinary(account);

        await using var stream = file.OpenReadStream();
        var publicId = $"mindflow/{userId}/{Guid.NewGuid()}";

        var resourceType = type switch
        {
            "image" => ResourceType.Image,
            "video" or "audio" => ResourceType.Video,
            _ => ResourceType.Raw
        };

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = publicId,
            Type = "upload"
        };

        RawUploadResult result;
        if (resourceType == ResourceType.Image)
        {
            result = await cloudinary.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId
            });
        }
        else if (resourceType == ResourceType.Video)
        {
            result = await cloudinary.UploadAsync(new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId
            });
        }
        else
        {
            result = await cloudinary.UploadAsync(uploadParams);
        }

        if (result.Error != null)
            throw new InvalidOperationException($"Error al subir archivo: {result.Error.Message}");

        return (result.SecureUrl.ToString(), type);
    }
}
