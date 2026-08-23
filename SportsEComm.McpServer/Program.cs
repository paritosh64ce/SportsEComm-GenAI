using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Backend API base URL is read from environment variable BACKEND_API_URL (default: http://localhost:5000)
var backendApiUrl = Environment.GetEnvironmentVariable("BACKEND_API_URL") ?? "http://localhost:5000";

builder.Services.AddHttpClient("backend", client => client.BaseAddress = new Uri(backendApiUrl));

var app = builder.Build();

// Helper to create a backend client and forward Authorization header (if present)
HttpClient CreateBackendClient(IHttpClientFactory httpFactory, HttpRequest? request = null)
{
    var client = httpFactory.CreateClient("backend");
    if (request != null && request.Headers.TryGetValue("Authorization", out var authValues))
    {
        // Forward the Authorization header as-is
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", (string)authValues);
    }
    return client;
}

app.MapGet("/tools/list_products", async (IHttpClientFactory httpFactory) =>
{
    var client = httpFactory.CreateClient("backend");
    var products = await client.GetFromJsonAsync<object>("api/products");
    return Results.Ok(products);
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
    var client = CreateBackendClient(httpFactory);
    var resp = await client.PostAsJsonAsync("api/customers/login", payload!);
    var content = await resp.Content.ReadAsStringAsync();
    request.HttpContext.Response.StatusCode = (int)resp.StatusCode;
    var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json";
    return Results.Content(content, contentType);
});

// Cart endpoints - require Authorization header from caller; forward header to backend
app.MapGet("/tools/cart", async (HttpRequest request, IHttpClientFactory httpFactory) =>
{
    var client = CreateBackendClient(httpFactory, request);
    var resp = await client.GetAsync("api/cart");
    var content = await resp.Content.ReadAsStringAsync();
    request.HttpContext.Response.StatusCode = (int)resp.StatusCode;
    var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json";
    return Results.Content(content, contentType);
});

app.MapPost("/tools/cart", async (HttpRequest request, IHttpClientFactory httpFactory) =>
{
    var payload = await request.ReadFromJsonAsync<object>();
    var client = CreateBackendClient(httpFactory, request);
    var resp = await client.PostAsJsonAsync("api/cart", payload!);
    var content = await resp.Content.ReadAsStringAsync();
    request.HttpContext.Response.StatusCode = (int)resp.StatusCode;
    var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json";
    return Results.Content(content, contentType);
});

app.MapDelete("/tools/cart/items/{productId}", async (int productId, HttpRequest request, IHttpClientFactory httpFactory) =>
{
    var client = CreateBackendClient(httpFactory, request);
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

app.MapGet("/", () => Results.Text("SportsEComm MCP Server - tools proxy. Configure BACKEND_API_URL to point to the backend API."));

app.Run();

