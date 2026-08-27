using System.Text.Json;
using System.Text.RegularExpressions;

namespace SportsEComm.Chatbot.Common
{
    public class ConversationManager
    {
        private readonly ModelClient _modelClient;
        private readonly McpClient _mcpClient;
        private readonly string _systemPrompt;
        private string? _jwt;

        public ConversationManager(ModelClient modelClient, McpClient mcpClient, string systemPrompt)
        {
            _modelClient = modelClient;
            _mcpClient = mcpClient;
            _systemPrompt = systemPrompt;
        }

        public void SetJwt(string jwt) => _jwt = jwt;

        public async Task<string> HandleUserMessageAsync(string userMessage)
        {
            // Build prompt: system + user
            var prompt = PromptTemplates.BuildSystemPrompt(_systemPrompt, userMessage);
            var modelOutput = await _modelClient.GenerateAsync(prompt);

            // Try extract tool_call JSON
            if (TryExtractToolCall(modelOutput, out var toolName, out var argsRaw))
            {
                string toolResultRaw;

                // Call MCP tool directly. Prefer server-side search_products (now implemented).
                toolResultRaw = await _mcpClient.InvokeToolAsync(toolName!, argsRaw ?? "{}", _jwt);

                // Build follow-up prompt with tool result
                var followUp = PromptTemplates.BuildFollowUpPrompt(_systemPrompt, userMessage, toolName!, toolResultRaw);
                var final = await _modelClient.GenerateAsync(followUp);
                return final;
            }

            // No tool call — return model output
            return modelOutput;
        }

        private bool TryExtractToolCall(string modelOutput, out string? toolName, out string? argsRaw)
        {
            toolName = null;
            argsRaw = null;

            // Find a JSON object that contains "tool_call"
            var m = Regex.Match(modelOutput, @"\{\s*\""tool_call\""[\s\S]*\}", RegexOptions.Singleline);
            if (!m.Success)
            {
                return false;
            }

            var jsonText = m.Value;
            try
            {
                using var doc = JsonDocument.Parse(jsonText);
                var root = doc.RootElement;
                if (root.TryGetProperty("tool_call", out var tc))
                {
                    if (tc.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    {
                        toolName = nameProp.GetString();
                    }

                    if (tc.TryGetProperty("args", out var argsProp))
                    {
                        argsRaw = argsProp.GetRawText();
                    }

                    return !string.IsNullOrEmpty(toolName);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
