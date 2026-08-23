using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SportsEComm.McpServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Backend API base URL is read from environment variable BACKEND_API_URL (default: http://localhost:5000)
var backendApiUrl = Environment.GetEnvironmentVariable("BACKEND_API_URL") ?? "http://localhost:5000";

builder.Services.AddHttpClient("backend", client => client.BaseAddress = new Uri(backendApiUrl));

var app = builder.Build();

// Map tool endpoints (moved to Extensions/ToolsEndpoints.cs)
app.MapToolEndpoints();

app.Run();
