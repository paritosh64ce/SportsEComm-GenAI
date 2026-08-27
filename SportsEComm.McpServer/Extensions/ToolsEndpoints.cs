using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using System;
using SportsEComm.McpServer.Extensions;

namespace SportsEComm.McpServer.Extensions
{
    internal static class ToolsEndpoints
    {
        public static void MapToolEndpoints(this WebApplication app)
        {
            app.MapGet("/tools/list_products", async (IHttpClientFactory httpFactory) =>
            {
                var client = httpFactory.CreateClient("backend");
                var products = await client.GetFromJsonAsync<object>("api/products");
                return Results.Ok(products);
            });

            // Server-side product search — accepts JSON body { query?: string, maxPrice?: number, category?: string }
            app.MapPost("/tools/search_products", async (HttpRequest request, IHttpClientFactory httpFactory) =>
            {
                // Read incoming args as raw string and parse robustly (handles JSON strings containing escaped JSON)
                JsonElement payload;
                string rawBody;
                using (var sr = new System.IO.StreamReader(request.Body))
                {
                    rawBody = await sr.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(rawBody))
                {
                    payload = JsonDocument.Parse("{}").RootElement;
                }
                else
                {
                    try
                    {
                        // If body is a quoted JSON string (e.g. "\"{\\\"query\\\":\\\"cricket\\\"}\""), Deserialize to get inner string
                        if (rawBody.StartsWith("\"") && rawBody.EndsWith("\""))
                        {
                            var inner = JsonSerializer.Deserialize<string>(rawBody);
                            payload = string.IsNullOrWhiteSpace(inner) ? JsonDocument.Parse("{}").RootElement : JsonDocument.Parse(inner).RootElement;
                        }
                        else
                        {
                            payload = JsonDocument.Parse(rawBody).RootElement;
                        }
                    }
                    catch
                    {
                        // Fallback to empty object on any parse errors
                        payload = JsonDocument.Parse("{}").RootElement;
                    }
                }

                string? query = null;
                decimal? maxPrice = null;
                string? category = null;
                if (payload.ValueKind != JsonValueKind.Undefined && payload.ValueKind != JsonValueKind.Null)
                {
                    if (payload.TryGetProperty("query", out var q) && q.ValueKind == JsonValueKind.String) query = q.GetString();
                    if (payload.TryGetProperty("maxPrice", out var mp) && mp.ValueKind == JsonValueKind.Number) { if (mp.TryGetDecimal(out var dec)) maxPrice = dec; }
                    if (payload.TryGetProperty("category", out var c) && c.ValueKind == JsonValueKind.String) category = c.GetString();
                }

                var client = httpFactory.CreateClient("backend");
                var productsElem = await client.GetFromJsonAsync<JsonElement>("api/products");

                if (productsElem.ValueKind != JsonValueKind.Array) return Results.Ok(productsElem);

                var results = new List<JsonElement>();
                foreach (var item in productsElem.EnumerateArray())
                {
                    bool include = true;
                    if (!string.IsNullOrEmpty(query))
                    {
                        var name = item.TryGetProperty("name", out var np) && np.ValueKind == JsonValueKind.String ? np.GetString() : "";
                        var desc = item.TryGetProperty("description", out var dp) && dp.ValueKind == JsonValueKind.String ? dp.GetString() : "";
                        if ((name == null || !name.Contains(query, StringComparison.OrdinalIgnoreCase)) &&
                            (desc == null || !desc.Contains(query, StringComparison.OrdinalIgnoreCase)))
                        {
                            include = false;
                        }
                    }
                    if (maxPrice.HasValue)
                    {
                        if (!(item.TryGetProperty("price", out var pp) && pp.ValueKind==JsonValueKind.Number && pp.GetDecimal() <= maxPrice.Value))
                            include = false;
                    }
                    if (!string.IsNullOrEmpty(category))
                    {
                        var cat = item.TryGetProperty("category", out var cp) && cp.ValueKind == JsonValueKind.String ? cp.GetString() : "";
                        if (cat == null || !cat.Equals(category, StringComparison.OrdinalIgnoreCase)) include = false;
                    }
                    if (include) results.Add(item);
                }

                return Results.Ok(results);
            });

            app.MapGet("/tools/get_customer_orders/{customerId}", async (string customerId, IHttpClientFactory httpFactory) =>
            {
                var client = httpFactory.CreateClient("backend");
                var orders = await client.GetFromJsonAsync<object>($"api/orders/{customerId}");
                return Results.Ok(orders);
            });

            // Customer login - proxies to POST /api/customers/login
            app.MapPost("/tools/customer_login", async (HttpRequest request, IHttpClientFactory httpFactory) =>
            {
                var payload = await request.ReadFromJsonAsync<object>();
                var client = BackendClientFactory.CreateBackendClient(httpFactory);
                var resp = await client.PostAsJsonAsync("api/customers/login", payload!);
                var content = await resp.Content.ReadAsStringAsync();
                request.HttpContext.Response.StatusCode = (int)resp.StatusCode;
                var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json";
                return Results.Content(content, contentType);
            });

            // Cart endpoints - require Authorization header from caller; forward header to backend
            app.MapGet("/tools/cart", async (HttpRequest request, IHttpClientFactory httpFactory) =>
            {
                var client = BackendClientFactory.CreateBackendClient(httpFactory, request);
                var resp = await client.GetAsync("api/cart");
                var content = await resp.Content.ReadAsStringAsync();
                request.HttpContext.Response.StatusCode = (int)resp.StatusCode;
                var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json";
                return Results.Content(content, contentType);
            });

            app.MapPost("/tools/cart", async (HttpRequest request, IHttpClientFactory httpFactory) =>
            {
                var payload = await request.ReadFromJsonAsync<object>();
                var client = BackendClientFactory.CreateBackendClient(httpFactory, request);
                var resp = await client.PostAsJsonAsync("api/cart", payload!);
                var content = await resp.Content.ReadAsStringAsync();
                request.HttpContext.Response.StatusCode = (int)resp.StatusCode;
                var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json";
                return Results.Content(content, contentType);
            });

            app.MapDelete("/tools/cart/items/{productId}", async (int productId, HttpRequest request, IHttpClientFactory httpFactory) =>
            {
                var client = BackendClientFactory.CreateBackendClient(httpFactory, request);
                var resp = await client.DeleteAsync($"api/cart/items/{productId}");
                var content = await resp.Content.ReadAsStringAsync();
                request.HttpContext.Response.StatusCode = (int)resp.StatusCode;
                var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json";
                return Results.Content(content, contentType);
            });

            app.MapPost("/tools/place_order", async (HttpRequest request, IHttpClientFactory httpFactory) =>
            {
                var payload = await request.ReadFromJsonAsync<object>();
                var client = httpFactory.CreateClient("backend");
                var resp = await client.PostAsJsonAsync("api/orders", payload!);
                var content = await resp.Content.ReadAsStringAsync();
                // Propagate status code to the outgoing response and return body and content-type
                request.HttpContext.Response.StatusCode = (int)resp.StatusCode;
                var contentType = resp.Content.Headers.ContentType?.ToString() ?? "text/plain";
                return Results.Content(content, contentType);
            });

            app.MapGet("/", () => Results.Text("SportsEComm MCP Server - tools proxy. Configure BACKEND_API_URL or BackendApi:Url in appsettings.json to point to the backend API (default http://localhost:5000). MCP server typically runs at http://localhost:6000."));
        }
    }
}
