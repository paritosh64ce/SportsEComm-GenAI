# Chatbot

`SportsEComm.Chatbot` is a .NET 10 console application that wires together a **Small Language Model (SLM)** — specifically **Mistral 7B** served locally via Ollama — and the MCP server to deliver a natural-language shopping assistant.

> **Why an SLM?** Mistral 7B runs on consumer hardware (CPU or a modest GPU) with no cloud dependency, no API key, and no data leaving the machine. It's compact enough to be practical and capable enough to understand shopping intent and emit structured tool-call JSON.

---

## How it works

The chatbot uses a **prompt-then-act** loop:

```
1. User sends a message
2. ConversationManager builds a prompt (system context + history + user message)
3. ModelClient posts it to Ollama — waits for a full (non-streaming) response
4. Response is inspected for a tool_call JSON block
   ├─ Tool call found →
   │     Parse tool name and args
   │     Call McpClient.InvokeToolAsync → MCP Server → Backend API
   │     Append tool result to the prompt context
   │     Ask the model again for a user-facing answer
   └─ No tool call → print the model response directly
5. Loop back to step 1
```

---

## Key classes (`SportsEComm.Chatbot.Common`)

### `ModelClient`
- Posts prompts to `http://localhost:11434/api/generate` with `stream: false`
- Parses the `response` field from the Ollama JSON reply
- Configured via `Ollama:BaseUrl` and `Ollama:Model` in `appsettings.json`

### `McpClient`
- Sends HTTP requests to the MCP server tool endpoints
- Accepts a raw JSON string as the tool arguments body
- Attaches `Authorization: Bearer <jwt>` when a token is supplied

### `ConversationManager`
- Maintains the conversation prompt history across turns
- Detects `{"tool_call":{"name":"...","args":{...}}}` in model output
- Orchestrates the model → tool → model cycle

---

## Console commands

| Input | Effect |
|-------|--------|
| Any message | Sent to the model; may trigger a tool call |
| `/login <email> <secretKey>` | Authenticates the user; stores the JWT for subsequent tool calls |
| `/exit` | Quits the app |

---

## Configuration (`appsettings.json`)

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434/api/generate",
    "Model": "mistral:7b"
  },
  "Mcp": {
    "BaseUrl": "http://localhost:6000"
  }
}
```

---

## Example conversation

```
You: show me cricket bats under 10000
Assistant: [calls search_products] Here are the bats under ₹10,000: ...

You: /login ms.dhoni@teamindia2011.com dhoni7#cup
Logged in as MS Dhoni.

You: add the first bat to my cart
Assistant: [calls add_to_cart] Added to your cart!

You: place my order
Assistant: [calls place_order] Order placed successfully. Order ID: 3.
```

---

## Characteristics of running a local SLM

- **Tool-call reliability**: Mistral 7B follows the `{"tool_call":{...}}` format reliably for clear, specific requests. Vague prompts may return free-form text instead — rephrasing usually resolves this
- **No cloud, no API key**: the model runs entirely on your machine; no data leaves the local network
- **Single-session memory**: conversation context resets when the console app restarts
- **No streaming**: the full response arrives once Ollama finishes generating (`stream: false`)
- **Swappable model**: any Ollama-compatible model can be used — update `Model:Name` in `appsettings.json`
