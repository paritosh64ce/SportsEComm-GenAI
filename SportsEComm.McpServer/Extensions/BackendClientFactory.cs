using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SportsEComm.McpServer.Extensions
{
    internal static class BackendClientFactory
    {
        public static HttpClient CreateBackendClient(IHttpClientFactory httpFactory, HttpRequest? request = null)
        {
            var client = httpFactory.CreateClient("backend");
            if (request != null && request.Headers.TryGetValue("Authorization", out StringValues authValues))
            {
                // Forward the Authorization header as-is
                client.DefaultRequestHeaders.Remove("Authorization");
                // Use ToString() to avoid nullable conversion warnings
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authValues.ToString());
            }
            return client;
        }
    }
}
