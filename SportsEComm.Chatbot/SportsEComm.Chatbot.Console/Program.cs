using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SportsEComm.Chatbot.Common;

Console.WriteLine("SportsEComm Chatbot (Console) - Phase 1 starter\nType /exit to quit, /login to authenticate (optional)\n");

// Load configuration from appsettings.json in the app folder
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var modelEndpoint = config.GetValue<string>("Model:Endpoint");
var modelName = config.GetValue<string>("Model:Name");
var mcpBase = config.GetValue<string>("MCP:BaseUrl");
var systemPrompt = config.GetValue<string>("SystemPrompt");

if (string.IsNullOrEmpty(modelEndpoint) || string.IsNullOrEmpty(modelName) || string.IsNullOrEmpty(mcpBase) || string.IsNullOrEmpty(systemPrompt))
{
    Console.WriteLine("Configuration is missing required values in appsettings.json. Please update Model:Endpoint, Model:Name, MCP:BaseUrl and SystemPrompt.");
    return;
}

var http = new HttpClient();
var modelClient = new ModelClient(http, endpoint: modelEndpoint, modelName: modelName);
var mcpClient = new McpClient(http, baseUrl: mcpBase);

var convo = new ConversationManager(modelClient, mcpClient, systemPrompt);

string? line;
while (true)
{
    Console.Write("You: ");
    line = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(line))
        continue;

    if (line.Equals("/exit", StringComparison.OrdinalIgnoreCase))
        break;

    if (line.StartsWith("/login", StringComparison.OrdinalIgnoreCase))
    {
        // syntax: /login email password
        var parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            Console.WriteLine("Usage: /login <email> <password>");
            continue;
        }

        var email = parts[1];
        var pwd = parts[2];

        // call MCP login tool
        var loginArgsJson = $"{{\"email\":\"{email}\",\"secretKey\":\"{pwd}\"}}";
        try
        {
            var raw = await mcpClient.InvokeToolAsync("customer_login", loginArgsJson);
            // try parse JWT from response (expecting JSON with token or jwt)
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("token", out var tokenProp) || doc.RootElement.TryGetProperty("jwt", out tokenProp))
                {
                    var token = tokenProp.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        convo.SetJwt(token);
                        Console.WriteLine("Login successful. Token saved in session.");
                        continue;
                    }
                }

                // fallback: print raw
                Console.WriteLine("Login response: " + raw);
            }
            catch
            {
                Console.WriteLine("Login response (non-json): " + raw);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Login failed: " + ex.ToString());
        }

        continue;
    }

    // Normal message
    try
    {
        var reply = await convo.HandleUserMessageAsync(line);
        Console.WriteLine("Assistant: " + reply);
    }
    catch (Exception ex)
    {
        // Print full exception details for debugging during Phase 1 smoke tests
        Console.WriteLine("Error: " + ex.ToString());
    }
}

Console.WriteLine("Goodbye!");

