using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mindflow_backend.AiIntegration.Application.Services;
using Mindflow_backend.Analytics.Domain.Entities;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using JournalEntry = Mindflow_backend.Journal.Domain.Entities.JournalEntry;

namespace Mindflow_backend.Analytics.Application.Services;

public class AnalyticsComputationService(AppDbContext dbContext, IAiService aiService)
{
    public async Task<AnalyticsCache> ComputeAndSaveWeeklyAsync(int userId, DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(6);

        var entries = await dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= weekStart && e.Date <= weekEnd)
            .ToListAsync();

        var score = CalculateScore(entries);

        var prevWeekStart = weekStart.AddDays(-7);
        var prevEntries = await dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= prevWeekStart && e.Date < weekStart)
            .ToListAsync();
        var prevScore = CalculateScore(prevEntries);
        var trend = CalculateTrend(prevScore, score);

        var fluctuationData = BuildFluctuationData(entries, weekStart);
        var trendData = await BuildTrendDataAsync(userId, weekStart);
        var kpis = BuildKpis(entries, score);
        var aiInsight = await GenerateAiInsightAsync(entries, score);

        var existing = await dbContext.AnalyticsCaches
            .FirstOrDefaultAsync(c => c.UserId == userId && c.WeekStart == weekStart);

        if (existing is not null)
        {
            existing.Score = score;
            existing.TrendPercentage = trend;
            existing.StartDate = weekStart;
            existing.EndDate = weekEnd;
            existing.AiInsight = aiInsight.english;
            existing.AiInsightLocalized = aiInsight.spanish;
            existing.Kpis = kpis;
            existing.FluctuationData = fluctuationData;
            existing.TrendData = trendData;
        }
        else
        {
            existing = new AnalyticsCache
            {
                UserId = userId,
                WeekStart = weekStart,
                Score = score,
                TrendPercentage = trend,
                StartDate = weekStart,
                EndDate = weekEnd,
                AiInsight = aiInsight.english,
                AiInsightLocalized = aiInsight.spanish,
                Kpis = kpis,
                FluctuationData = fluctuationData,
                TrendData = trendData
            };
            dbContext.AnalyticsCaches.Add(existing);
        }

        await dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<List<object>> ComputeMoodCalendarAsync(int userId, int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var entries = await dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= start && e.Date <= end)
            .ToListAsync();

        var grouped = entries
            .GroupBy(e => e.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var days = new List<object>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            if (!grouped.TryGetValue(d, out var dayEntries))
            {
                days.Add(new { date = d.ToString("yyyy-MM-dd"), sentiment = (string?)null, color = (string?)null, entryCount = 0 });
                continue;
            }

            var dominant = dayEntries
                .GroupBy(e => e.Sentiment?.ToLower() ?? "neutral")
                .OrderByDescending(g => g.Count())
                .First().Key;

            var color = dominant switch
            {
                "positive" => "#4CAF50",
                "negative" => "#F44336",
                _ => "#FF9800"
            };

            days.Add(new { date = d.ToString("yyyy-MM-dd"), sentiment = dominant, color, entryCount = dayEntries.Count });
        }

        return days;
    }

    public async Task<WordCloud> ComputeAndSaveWordCloudAsync(int userId)
    {
        var entries = await dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Date)
            .Take(100)
            .ToListAsync();

        var words = ExtractWordCloud(entries);
        var wordsJson = JsonSerializer.Serialize(words);

        var existing = await dbContext.WordClouds
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (existing is not null)
        {
            existing.Words = wordsJson;
        }
        else
        {
            existing = new WordCloud { UserId = userId, Words = wordsJson };
            dbContext.WordClouds.Add(existing);
        }

        await dbContext.SaveChangesAsync();
        return existing;
    }

    private static int CalculateScore(List<JournalEntry> entries)
    {
        if (entries.Count == 0) return 0;
        var total = entries.Sum(e => e.Sentiment?.ToLower() switch
        {
            "positive" => 100,
            "negative" => 0,
            _ => 50
        });
        return total / entries.Count;
    }

    private static string CalculateTrend(int prevScore, int currentScore)
    {
        if (prevScore == 0 && currentScore == 0) return "+0%";
        if (prevScore == 0) return "+100%";
        var diff = ((double)currentScore - prevScore) / prevScore * 100;
        return $"{(diff >= 0 ? "+" : "")}{diff:F0}%";
    }

    private static string BuildFluctuationData(List<JournalEntry> entries, DateOnly weekStart)
    {
        var dayScores = Enumerable.Range(0, 7).Select(day =>
        {
            var date = weekStart.AddDays(day);
            var dayEntries = entries.Where(e => e.Date == date).ToList();
            return dayEntries.Count > 0
                ? dayEntries.Average(e => e.Sentiment?.ToLower() switch
                {
                    "positive" => 10,
                    "negative" => 2,
                    _ => 6
                })
                : 5.0;
        }).ToList();

        var labels = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };

        var chart = new
        {
            labels_keys = (string[]?)null,
            labels,
            dataset_label = "Fluctuación Emocional",
            datasets = new[]
            {
                new
                {
                    label_key = (string?)"analytics.chart.fluctuation",
                    label = "Fluctuación Emocional",
                    data = dayScores,
                    backgroundColor = "var(--accent-primary)",
                    hoverBackgroundColor = "var(--accent-primary-hover)",
                    borderRadius = 6,
                    borderSkipped = false,
                    barPercentage = 0.6,
                    categoryPercentage = 0.8,
                    fill = true,
                    borderColor = "var(--accent-primary)",
                    tension = 0.35,
                    pointBackgroundColor = "#ffffff",
                    pointBorderColor = "var(--accent-primary)",
                    pointBorderWidth = 2,
                    pointRadius = 4
                }
            }
        };

        return JsonSerializer.Serialize(chart);
    }

    private async Task<string> BuildTrendDataAsync(int userId, DateOnly weekStart)
    {
        var rangeStart = weekStart.AddDays(-3 * 7);
        var rangeEnd = weekStart.AddDays(6);

        var allEntries = await dbContext.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.Date >= rangeStart && e.Date <= rangeEnd)
            .ToListAsync();

        var scores = new List<double>();
        var labels = new List<string>();

        for (var i = 3; i >= 0; i--)
        {
            var start = weekStart.AddDays(-i * 7);
            var end = start.AddDays(6);
            var weekEntries = allEntries.Where(e => e.Date >= start && e.Date <= end).ToList();

            var score = weekEntries.Count > 0
                ? weekEntries.Average(e => e.Sentiment?.ToLower() switch
                {
                    "positive" => 10,
                    "negative" => 2,
                    _ => 6
                })
                : 5.0;

            scores.Add(Math.Round(score, 1));
            labels.Add($"Sem {4 - i}");
        }

        var chart = new
        {
            labels_keys = (string[]?)null,
            labels,
            datasets = new[]
            {
                new
                {
                    label_key = (string?)"analytics.chart.mindfulness",
                    label = "Mindfulness",
                    data = scores,
                    backgroundColor = "var(--accent-success)",
                    borderColor = "var(--accent-success)",
                    fill = true,
                    tension = 0.35,
                    pointBackgroundColor = "#ffffff",
                    pointBorderColor = "var(--accent-success)",
                    pointBorderWidth = 2,
                    pointRadius = 4
                }
            }
        };

        return JsonSerializer.Serialize(chart);
    }

    private static string BuildKpis(List<JournalEntry> entries, int score)
    {
        var moodScore = entries.Count > 0
            ? entries.Average(e => e.Sentiment?.ToLower() switch
            {
                "positive" => 10,
                "negative" => 2,
                _ => 6
            })
            : 5.0;

        var kpis = new[]
        {
            new
            {
                label_key = (string?)null,
                label = "Estado de Ánimo",
                value_key = (string?)null,
                value = $"{moodScore:F1}",
                unit_key = (string?)null,
                unit = "/10",
                color_class = "border-orange"
            },
            new
            {
                label_key = (string?)null,
                label = "Entradas Registradas",
                value_key = (string?)null,
                value = entries.Count.ToString(),
                unit_key = (string?)null,
                unit = "esta semana",
                color_class = "border-blue"
            },
            new
            {
                label_key = (string?)null,
                label = "Puntaje General",
                value_key = (string?)null,
                value = score.ToString(),
                unit_key = (string?)null,
                unit = "/100",
                color_class = "border-green"
            }
        };

        return JsonSerializer.Serialize(kpis);
    }

    private async Task<(string english, string spanish)> GenerateAiInsightAsync(
        List<JournalEntry> entries, int score)
    {
        var count = entries.Count;
        var mood = score >= 70 ? "positive" : score >= 40 ? "neutral" : "low";

        string en = mood switch
        {
            "positive" => $"Great week! You recorded {count} entries with a generally positive mood. Keep up the good habits.",
            "low" => $"This week you wrote {count} entries. We noticed some challenging emotions. Consider taking time for self-care.",
            _ => count == 0
                ? "No entries recorded this week. Start writing to track your emotional well-being."
                : $"You recorded {count} entries this week with a balanced mood. Consistent reflection builds emotional awareness."
        };

        string es;
        if (count > 0)
        {
            var contents = entries.Select(e => e.Content);
            var geminiResponse = await aiService.GenerateWeeklySummaryAsync(contents, score);
            es = string.IsNullOrEmpty(geminiResponse)
                ? FallbackSpanish(count, mood)
                : geminiResponse;
        }
        else
        {
            es = "No registraste entradas esta semana. Empieza a escribir para monitorear tu bienestar emocional.";
        }

        return (en, es);
    }

    private static string FallbackSpanish(int count, string mood) => mood switch
    {
        "positive" => $"¡Gran semana! Registraste {count} entradas con un estado de ánimo generalmente positivo. Sigue con esos buenos hábitos.",
        "low" => $"Esta semana escribiste {count} entradas. Notamos algunas emociones desafiantes. Considera tomarte tiempo para cuidarte.",
        _ => $"Registraste {count} entradas esta semana con un estado de ánimo equilibrado. La reflexión constante fortalece la conciencia emocional."
    };

    private static List<object> ExtractWordCloud(List<JournalEntry> entries)
    {
        var stopWords = new HashSet<string>
        {
            "el", "la", "los", "las", "y", "e", "de", "del", "en", "un", "una",
            "que", "es", "no", "lo", "le", "se", "con", "por", "para", "su", "al",
            "como", "más", "pero", "sus", "este", "entre", "ya", "todo", "también",
            "fue", "era", "muy", "sin", "cuando", "donde", "bien", "así", "después",
            "cada", "otro", "esa", "ese", "eso", "esto", "son", "ser", "había",
            "tenía", "sido", "vez", "dos", "tan", "cual", "solo", "nada", "algo",
            "ello", "tanto", "nunca", "siempre", "casi", "además", "aunque", "sino",
            "menos", "dentro", "fuera", "tras", "contra", "hacia", "sobre", "ante",
            "bajo", "durante", "the", "and", "for", "are", "but", "not", "you",
            "all", "can", "had", "her", "was", "one", "our", "out", "has", "have",
            "been", "some", "them", "than", "its", "over", "such", "that", "this",
            "with", "will", "would", "about", "into", "could", "other", "their",
            "there", "these", "which", "your", "when", "also", "how", "what",
            "then", "many", "some", "those", "very", "just", "should", "after",
        };

        var wordCounts = new Dictionary<string, int>();

        foreach (var entry in entries)
        {
            var words = entry.Content.Split(
                [' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']', '-'],
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var cleaned = word.Trim().ToLowerInvariant();
                if (cleaned.Length <= 2 || stopWords.Contains(cleaned) || cleaned.All(char.IsDigit))
                    continue;

                wordCounts.TryGetValue(cleaned, out var count);
                wordCounts[cleaned] = count + 1;
            }
        }

        if (wordCounts.Count == 0) return [];

        var maxCount = wordCounts.Values.Max();

        return wordCounts
            .OrderByDescending(kv => kv.Value)
            .Take(20)
            .Select(kv => (object)new { tag = kv.Key, score = Math.Round((double)kv.Value / maxCount, 2) })
            .ToList();
    }
}