# Architecture

The system is a three-tier pipeline where a **Small Language Model (SLM)** running locally orchestrates shopping actions through a controlled tool proxy, which in turn calls a real backend API.

> **SLM choice**: Mistral 7B (7 B parameters) via Ollama. Small enough to run on a laptop CPU/GPU, capable enough to understand intent and emit structured tool-call JSON.

```
User
 │
 ▼
SportsEComm.Chatbot.Console   ← console UI, session state
 │
 ├─► Ollama / Mistral 7B (SLM) ← language understanding, tool-call decisions
 │
 └─► SportsEComm.McpServer     ← HTTP tool proxy  (http://localhost:6000)
          │
          └─► SportsEComm.Api  ← REST API, EF Core, SQL Server LocalDB  (http://localhost:5000)
```

---

## Components

### SportsEComm.Api
- **Framework**: .NET 10 minimal Web API with MVC controllers
- **Database**: SQL Server LocalDB via Entity Framework Core 10
- **Auth**: JWT Bearer (`JwtSettings` in `appsettings.json`), passwords hashed with `PasswordHasher<T>`
- **Patterns**: Repository + Unit of Work, service layer, DTOs

Internal layers:

```
Controllers/    → REST surface (presentation)
Services/       → business logic
Repositories/   → IRepository<T> + IUnitOfWork (data access)
Data/           → DbContext + DbInitializer (auto-seed on startup)
Models/         → Product, Customer, Cart, CartItem, Order, OrderItem
DTOs/           → request / response shapes
Extensions/     → Swagger + JWT setup helpers
```

### SportsEComm.McpServer
- **Framework**: .NET 10 minimal API — HTTP only
- **Role**: tool proxy; maps `/tools/*` routes to backend API calls and forwards `Authorization` headers
- **Config**: `BackendApi:Url` in `appsettings.json` (default `http://localhost:5000`) or env var `BACKEND_API_URL`

### SportsEComm.Chatbot.Common
- **`ModelClient`** — sends prompts to Ollama `/api/generate` (non-streaming), parses `{"response":"..."}` field
- **`McpClient`** — HTTP client for MCP tool endpoints; attaches JWT when provided
- **`ConversationManager`** — the orchestration loop: model → detect tool_call → MCP → model again → final answer

### SportsEComm.Chatbot.Console
- Entry point; reads `appsettings.json` for `Ollama:BaseUrl`, `Ollama:Model`, and `Mcp:BaseUrl`
- Handles `/login`, `/exit` slash commands
- Delegates everything else to `ConversationManager`

---

## Data flow — one user turn

```
1. User types a message
2. ConversationManager appends it to the prompt history and calls ModelClient.GenerateAsync
3. ModelClient posts the prompt to Ollama and returns the model response text
4. ConversationManager checks if the response contains {"tool_call":{...}}
   ├─ YES → extracts tool name and args, calls McpClient.InvokeToolAsync
   │         McpServer receives the call and proxies it to the API
   │         Tool result is injected back into the prompt as context
   │         ConversationManager calls the model again for the final answer
   └─ NO  → prints the response directly
```

---

## Design decisions

| Decision | Rationale |
|----------|-----------|
| HTTP-only for local demo | Avoids dev-cert issues; `UseHttpsRedirection` is commented out but preserved |
| JWT identity from token claims | Customer ID is never trusted from URL parameters — extracted from `HttpContext.User` |
| `ReferenceHandler.IgnoreCycles` | EF navigation properties create circular refs (`Cart → CartItem → Cart`); ignored at serialization |
| `MOCK_MCP` / `MOCK_MODEL` removed | Live Ollama + live API are available; mocks were dev scaffolding only |
| `stream: false` on Ollama | Simplifies response parsing; full text arrives in a single JSON object |

---

## Ports

| Service | Default port |
|---------|-------------|
| SportsEComm.Api | `http://localhost:5000` |
| SportsEComm.McpServer | `http://localhost:6000` |
| Ollama | `http://localhost:11434` |
