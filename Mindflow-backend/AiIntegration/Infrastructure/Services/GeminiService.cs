using System.Text;
using System.Text.Json;
using Mindflow_backend.AiIntegration.Application.Services;

namespace Mindflow_backend.AiIntegration.Infrastructure.Services;

public class GeminiService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<GeminiService> logger) : IAiService
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

        return await CallGeminiAsync(prompt);
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

        return await CallGeminiAsync(prompt);
    }

    private async Task<string> CallGeminiAsync(string prompt)
    {
        var apiKey = configuration["AiSettings:GeminiApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) return string.Empty;

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

            if (!response.IsSuccessStatusCode) return string.Empty;

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gemini API call failed — returning empty response.");
            return string.Empty;
        }
    }
}
