using System.Net.Http.Json;
using System;
using System.Text.Json;

namespace SportsEComm.Chatbot.Common
{
    public class ModelClient
    {
        private readonly HttpClient _http;
        private readonly string _endpoint;
        private readonly string _modelName;

        public ModelClient(HttpClient http, string endpoint = "http://localhost:11434/api/generate", string modelName = "mistral-7b")
        {
            _http = http;
            _endpoint = endpoint.TrimEnd('/');
            _modelName = modelName;
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var payload = new
            {
                model = _modelName,
                prompt = prompt,
                stream = false    // request a single complete JSON response (not NDJSON stream)
            };

            using var resp = await _http.PostAsJsonAsync(_endpoint, payload);
            var raw = await resp.Content.ReadAsStringAsync();

            // Try to parse common shapes conservatively
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                // Ollama /api/generate non-stream: { "response": "..." }
                if (root.TryGetProperty("response", out var responseProp) && responseProp.ValueKind == JsonValueKind.String)
                    return responseProp.GetString() ?? raw;

                // Common: { "text": "..." }
                if (root.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                    return textProp.GetString() ?? raw;

                // Common: { "choices": [ { "text": "..." } ] } (OpenAI / Ollama v1 style)
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var first = choices[0];
                    if (first.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                        return text.GetString() ?? raw;
                }

                // Common Ollama-like: { "results": [ { "content": "..." } ] }
                if (root.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
                {
                    var first = results[0];
                    if (first.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                        return content.GetString() ?? raw;
                }

                // Otherwise return raw JSON string
                return raw;
            }
            catch
            {
                // Not JSON — return raw
                return raw;
            }
        }
    }
}
