using System.Net.Http.Json;
using System.Text.Json;

namespace SportsEComm.Chatbot.Common
{
    public class McpClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public McpClient(HttpClient http, string baseUrl = "http://localhost:6000")
        {
            _http = http;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<string> InvokeToolAsync(string toolName, string argsJsonRaw, string? jwt = null)
        {
            var url = $"{_baseUrl}/tools/{toolName}";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (!string.IsNullOrEmpty(jwt))
            {
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
            }

            // Send raw JSON string content to avoid JsonDocument lifetime issues
            req.Content = new System.Net.Http.StringContent(argsJsonRaw ?? "{}", System.Text.Encoding.UTF8, "application/json");

            using var resp = await _http.SendAsync(req);
            var raw = await resp.Content.ReadAsStringAsync();
            return raw;
        }

        public async Task<string> GetListProductsAsync()
        {
            var url = $"{_baseUrl}/tools/list_products";
            using var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }
    }
}
