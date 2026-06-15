public class CreateJournalEntryRequest
{
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Sentiment { get; set; } = "neutral";
    public string Category { get; set; } = "Sin categoría";
}gi