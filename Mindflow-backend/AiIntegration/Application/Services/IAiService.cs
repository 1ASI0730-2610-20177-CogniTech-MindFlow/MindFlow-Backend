namespace Mindflow_backend.AiIntegration.Application.Services;

public interface IAiService
{
    Task<string> GenerateEmpathicResponseAsync(string content, string sentiment);
    Task<string> GenerateWeeklySummaryAsync(IEnumerable<string> contents, int score);
    Task<string> GenerateStressAdviceAsync(string stressLevel, int score, int entryCount, IEnumerable<string> habitNames);
    Task<string> GenerateHabitSuggestionsAsync(IEnumerable<string> currentHabits, int completionRate, string stressLevel, IEnumerable<string> recentJournalSnippets);
}
