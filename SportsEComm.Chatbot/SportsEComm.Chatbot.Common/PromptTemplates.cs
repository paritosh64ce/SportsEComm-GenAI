namespace SportsEComm.Chatbot.Common
{
    public static class PromptTemplates
    {
        public static string BuildSystemPrompt(string system, string userMessage)
        {
            return system + "\n\nUser: " + userMessage + "\nAssistant:";
        }

        public static string BuildFollowUpPrompt(string system, string originalUser, string toolName, string toolResultRaw)
        {
            return system + "\n\nUser: " + originalUser + "\nToolResult (" + toolName + "): " + toolResultRaw + "\nAssistant:";
        }
    }
}
