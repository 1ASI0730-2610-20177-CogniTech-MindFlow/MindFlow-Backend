namespace Mindflow_backend.Journal.Application.Dtos;

public class UpdateJournalEntryRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Sentiment { get; set; } = "neutral";
    public string Category { get; set; } = "Sin categoría";
}