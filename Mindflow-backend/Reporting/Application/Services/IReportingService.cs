namespace Mindflow_backend.Reporting.Application.Services;

public interface IReportingService
{
    Task<byte[]> GeneratePdfAsync(int userId);
    Task<byte[]> GenerateCsvAsync(int userId);
}
