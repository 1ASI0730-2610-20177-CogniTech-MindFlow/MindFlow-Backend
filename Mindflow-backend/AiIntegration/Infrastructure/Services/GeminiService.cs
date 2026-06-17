using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Mindflow_backend.AiIntegration.Application.Services;
using Mindflow_backend.AiIntegration.Domain.Model.Entities;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.AiIntegration.Infrastructure.Services;

public class GeminiService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<GeminiService> logger,
    AppDbContext dbContext) : IAiService
{
    private const string ApiBase =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public async Task<string> GenerateEmpathicResponseAsync(string content, string sentiment)
    {
        var snippet = content[..Math.Min(content.Length, 500)];
        var prompt = $"""
            Eres un asistente de bienestar mental empático. El usuario escribió en su diario con estado emocional '{sentiment}':

            "{snippet}"

            Proporciona una respuesta breve (2-3 oraciones), cálida y empática en español. Valida sus emociones sin juzgar. Sin encabezados ni listas.
            """;

        return await CallGeminiAsync(prompt, "empathic_response");
    }

    public async Task<string> GenerateWeeklySummaryAsync(IEnumerable<string> contents, int score)
    {
        var list = contents.ToList();
        if (list.Count == 0) return string.Empty;

        var snippets = string.Join("\n- ", list.Take(5).Select(c => c[..Math.Min(c.Length, 100)]));
        var prompt = $"""
            Eres un asistente de bienestar mental. El usuario registró {list.Count} entradas esta semana con un puntaje de bienestar de {score}/100.

            Fragmentos de sus entradas:
            - {snippets}

            Genera un resumen empático y motivador en español (3-4 oraciones) que refleje su semana emocional. Sin encabezados ni listas.
            """;

        return await CallGeminiAsync(prompt, "weekly_summary");
    }

    public async Task<string> GenerateStressAdviceAsync(
        string stressLevel, int score, int entryCount, IEnumerable<string> habitNames)
    {
        var habits = string.Join(", ", habitNames.Take(5));
        var prompt = $"""
            Eres un asistente de bienestar mental. El usuario tiene un nivel de estrés '{stressLevel}' (puntaje {score}/100) basado en {entryCount} entradas de diario recientes.
            Hábitos del usuario: {(string.IsNullOrEmpty(habits) ? "ninguno registrado" : habits)}.

            Genera un mensaje empático en español (2-3 oraciones) que:
            - Si el estrés es 'high': explique con calidez por qué pausamos temporalmente sus hábitos y cómo puede cuidarse.
            - Si el estrés es 'medium': anime a mantener rutinas con pequeños ajustes.
            - Si el estrés es 'low': celebre su bienestar y motive a continuar con sus hábitos.
            Sin encabezados ni listas.
            """;

        return await CallGeminiAsync(prompt, "stress_advice");
    }

    public async Task<string> GenerateHabitSuggestionsAsync(
        IEnumerable<string> currentHabits, int completionRate, string stressLevel, IEnumerable<string> recentJournalSnippets)
    {
        var habits = string.Join(", ", currentHabits.Take(10));
        var snippets = string.Join("\n- ", recentJournalSnippets.Take(5).Select(s => s[..Math.Min(s.Length, 100)]));
        var prompt =
            "Eres un asistente de bienestar mental. Analiza el perfil del usuario y sugiere 3 hábitos nuevos personalizados.\n\n" +
            $"Hábitos actuales: {(string.IsNullOrEmpty(habits) ? "ninguno" : habits)}\n" +
            $"Tasa de completado: {completionRate}%\n" +
            $"Nivel de estrés: {stressLevel}\n" +
            "Fragmentos recientes del diario:\n" +
            $"- {(string.IsNullOrEmpty(snippets) ? "sin entradas recientes" : snippets)}\n\n" +
            "Responde SOLO con un JSON array (sin markdown, sin backticks) con exactamente 3 objetos con las keys: name, category, frequency (Daily/Weekly/Monthly), reason (en español).";

        return await CallGeminiAsync(prompt, "habit_suggestions");
    }

    private async Task<string> CallGeminiAsync(string prompt, string operation)
    {
        var apiKey = configuration["AiSettings:GeminiApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) return string.Empty;

        var sw = Stopwatch.StartNew();
        string? errorMsg = null;
        var success = false;
        var responseText = string.Empty;

        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                },
                generationConfig = new { temperature = 0.7, maxOutputTokens = 250 }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var client = httpClientFactory.CreateClient("Gemini");
            var response = await client.PostAsync($"{ApiBase}?key={apiKey}", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                errorMsg = $"HTTP {(int)response.StatusCode}";
                return string.Empty;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            responseText = text?.Trim() ?? string.Empty;
            success = !string.IsNullOrEmpty(responseText);
            return responseText;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gemini API call failed — returning empty response.");
            errorMsg = ex.Message[..Math.Min(ex.Message.Length, 500)];
            return string.Empty;
        }
        finally
        {
            sw.Stop();
            await SaveMetricAsync(operation, (int)sw.ElapsedMilliseconds, success, prompt.Length, responseText.Length, errorMsg);
        }
    }

    private async Task SaveMetricAsync(string operation, int latencyMs, bool success, int promptLength, int responseLength, string? errorMessage)
    {
        try
        {
            dbContext.AiMetricLogs.Add(new AiMetricLog
            {
                Operation = operation,
                LatencyMs = latencyMs,
                Success = success,
                PromptLength = promptLength,
                ResponseLength = responseLength,
                ErrorMessage = errorMessage
            });
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to save AI metric log.");
        }
    }
}
