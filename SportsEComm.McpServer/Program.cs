using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SportsEComm.McpServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Backend API base URL is read from configuration (BackendApi:Url), fallback to environment variable BACKEND_API_URL, then a hardcoded default
var backendApiUrl = builder.Configuration["BackendApi:Url"]
                    ?? Environment.GetEnvironmentVariable("BACKEND_API_URL")
                    ?? "http://localhost:5000";

builder.Services.AddHttpClient("backend", client => client.BaseAddress = new Uri(backendApiUrl));

// Demo uses HTTP only — to call the API over HTTPS uncomment the block below and comment out the line above
// builder.Services.AddHttpClient("backend", client => client.BaseAddress = new Uri(backendApiUrl))
//     .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//     {
//         // Accept the local dev HTTPS certificate (self-signed) for localhost
//         ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//     });


var app = builder.Build();

// Map tool endpoints (moved to Extensions/ToolsEndpoints.cs)
app.MapToolEndpoints();

app.Run();
