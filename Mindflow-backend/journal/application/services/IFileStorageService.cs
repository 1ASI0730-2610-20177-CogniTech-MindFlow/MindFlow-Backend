namespace Mindflow_backend.Journal.Application.Services;

public interface IFileStorageService
{
    Task<(string Url, string Type)> SaveAsync(IFormFile file, int userId);
}
