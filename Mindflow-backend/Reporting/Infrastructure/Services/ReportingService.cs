using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Reporting.Application.Services;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Mindflow_backend.Reporting.Infrastructure.Services;

public class ReportingService(AppDbContext db) : IReportingService
{
    public async Task<byte[]> GeneratePdfAsync(int userId)
    {
        var entries = await db.JournalEntries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .Take(100)
            .ToListAsync();

        var totalEntries = entries.Count;
        var positiveCount = entries.Count(e => e.Sentiment == "positive");
        var negativeCount = entries.Count(e => e.Sentiment == "negative");
        var neutralCount = entries.Count(e => e.Sentiment == "neutral");

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("MindFlow")
                            .FontSize(22).Bold().FontColor(Color.FromHex("#4F46E5"));
                        row.ConstantItem(120).AlignRight().Text($"Generado: {DateTime.UtcNow:dd/MM/yyyy}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                    col.Item().Text("Reporte Personal de Bienestar")
                        .FontSize(13).SemiBold().FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Color.FromHex("#4F46E5"));
                });

                page.Content().PaddingTop(16).Column(col =>
                {
                    // Stats summary
                    col.Item().Background(Color.FromHex("#F5F3FF")).Padding(12).Row(statsRow =>
                    {
                        statsRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"{totalEntries}").FontSize(24).Bold().FontColor(Color.FromHex("#4F46E5"));
                            c.Item().Text("Entradas totales").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                        statsRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"{positiveCount}").FontSize(24).Bold().FontColor(Color.FromHex("#16A34A"));
                            c.Item().Text("Positivas").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                        statsRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"{neutralCount}").FontSize(24).Bold().FontColor(Color.FromHex("#CA8A04"));
                            c.Item().Text("Neutrales").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                        statsRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"{negativeCount}").FontSize(24).Bold().FontColor(Color.FromHex("#DC2626"));
                            c.Item().Text("Negativas").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });

                    col.Item().PaddingTop(16).Text("Historial de Entradas")
                        .FontSize(12).SemiBold();
                    col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    foreach (var entry in entries)
                    {
                        col.Item().PaddingTop(10).Column(entryCol =>
                        {
                            entryCol.Item().Row(row =>
                            {
                                row.RelativeItem().Text(entry.Title).SemiBold().FontSize(10);
                                row.ConstantItem(80).AlignRight()
                                    .Text(entry.Date.ToString("dd/MM/yyyy"))
                                    .FontSize(9).FontColor(Colors.Grey.Medium);
                            });

                            entryCol.Item().PaddingTop(2).Row(row =>
                            {
                                row.AutoItem().Text($"{entry.Category}")
                                    .FontSize(8).FontColor(Colors.Grey.Medium);
                                row.AutoItem().Text("  •  ").FontSize(8).FontColor(Colors.Grey.Medium);
                                var (sentimentText, sentimentColor) = entry.Sentiment switch
                                {
                                    "positive" => ("Positivo", Color.FromHex("#16A34A")),
                                    "negative" => ("Negativo", Color.FromHex("#DC2626")),
                                    _ => ("Neutral", Color.FromHex("#CA8A04"))
                                };
                                row.AutoItem().Text(sentimentText).FontSize(8).FontColor(sentimentColor);
                            });

                            var snippet = entry.Content.Length > 200
                                ? entry.Content[..200] + "…"
                                : entry.Content;

                            entryCol.Item().PaddingTop(3).Text(snippet)
                                .FontSize(9).FontColor(Colors.Grey.Darken1);
                            entryCol.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("MindFlow — ").FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<byte[]> GenerateCsvAsync(int userId)
    {
        var entries = await db.JournalEntries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .Take(500)
            .ToListAsync();

        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        csv.WriteHeader<JournalEntryCsvRow>();
        await csv.NextRecordAsync();

        foreach (var e in entries)
        {
            csv.WriteRecord(new JournalEntryCsvRow
            {
                Fecha = e.Date.ToString("yyyy-MM-dd"),
                Titulo = e.Title,
                Categoria = e.Category,
                Sentimiento = e.Sentiment,
                Contenido = e.Content,
                CreadoEn = e.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty
            });
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync();
        return ms.ToArray();
    }
}

public sealed class JournalEntryCsvRow
{
    public string Fecha { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Sentimiento { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string CreadoEn { get; set; } = string.Empty;
}
