namespace Mindflow_backend.AiIntegration.Application.Services;

public interface IAiService
{
    Task<string> GenerateEmpathicResponseAsync(string content, string sentiment);
    Task<string> GenerateWeeklySummaryAsync(IEnumerable<string> contents, int score);
}
