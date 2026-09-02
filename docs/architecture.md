# Architecture

The system is a three-tier pipeline where a **Small Language Model (SLM)** running locally orchestrates shopping actions through a controlled tool proxy, which in turn calls a real backend API.

> **SLM choice**: Mistral 7B (7 B parameters) via Ollama. Small enough to run on a laptop CPU/GPU, capable enough to understand intent and emit structured tool-call JSON.

---

## System overview

```mermaid
graph TB
    U(["👤 User"])

    subgraph Console ["SportsEComm.Chatbot.Console"]
        direction TB
        P["Program.cs\nconsole UI + slash commands"]
        CM["ConversationManager\norchestration loop"]
        MCL["ModelClient"]
        MCC["McpClient"]
        P --> CM
        CM --> MCL
        CM --> MCC
    end

    subgraph Ollama ["🧠 Ollama  ·  :11434"]
        SLM(["Mistral 7B\n(SLM — runs locally)"])
    end

    subgraph McpServer ["🔌 SportsEComm.McpServer  ·  :6000"]
        direction TB
        TE["ToolsEndpoints\n/tools/* routes"]
        BCF["BackendClientFactory\nforwards Auth header"]
        TE --> BCF
    end

    subgraph BackendApi ["⚙️ SportsEComm.Api  ·  :5000"]
        direction TB
        CTL["Controllers"]
        SVC["Services"]
        REPO["Repositories\nUnit of Work"]
        DB[("SQL Server\nLocalDB")]
        CTL --> SVC --> REPO --> DB
    end

    U -->|"message"| P
    MCL -->|"POST /api/generate\nstream: false"| SLM
    SLM -->|"{ response: ... }"| MCL
    MCC -->|"POST /tools/{name}"| TE
    BCF -->|"proxied HTTP"| CTL
    CM -->|"answer"| U
```

---

## Chatbot orchestration loop

Each user message goes through up to two model calls — one to determine intent (and possibly emit a tool call) and a second to compose the final answer from the tool result.

```mermaid
sequenceDiagram
    actor User
    participant CM  as ConversationManager
    participant ML  as ModelClient
    participant OL  as Ollama (Mistral 7B)
    participant MC  as McpClient
    participant MCP as McpServer /tools/*
    participant API as SportsEComm.Api

    User  ->>  CM  : user message
    CM    ->>  ML  : GenerateAsync(prompt)
    ML    ->>  OL  : POST /api/generate
    OL   -->>  ML  : { "response": "..." }
    ML   -->>  CM  : model output

    alt model emits tool_call JSON
        CM   ->>  MC  : InvokeToolAsync(tool, args, jwt?)
        MC   ->>  MCP : POST /tools/{name}
        MCP  ->>  API : proxied REST call
        API -->>  MCP : JSON result
        MCP -->>  MC  : tool result
        MC  -->>  CM  : raw result string
        CM   ->>  ML  : GenerateAsync(prompt + tool result)
        ML   ->>  OL  : POST /api/generate
        OL  -->>  ML  : { "response": "..." }
        ML  -->>  CM  : final answer
    end

    CM   -->>  User : print response
```

---

## API layer structure

```mermaid
graph LR
    subgraph API ["SportsEComm.Api"]
        direction TB
        C["Controllers\nProductsController\nCustomersController\nCartController\nOrdersController"]
        S["Services\nProductService\nCustomerService\nCartService\nOrderService\nAuthService"]
        R["Repositories\nIRepository&lt;T&gt;\nIUnitOfWork"]
        D["Data\nSportsECommContext\nDbInitializer"]
        DB[("SQL Server LocalDB\nSportsECommDb")]

        C --> S --> R --> D --> DB
    end
```

---

## Components

### SportsEComm.Api
- **Framework**: .NET 10 Web API with MVC controllers
- **Database**: SQL Server LocalDB via Entity Framework Core 10
- **Auth**: JWT Bearer — identity resolved from claims, never from URL params
- **Patterns**: Repository + Unit of Work, service layer, DTOs, `PasswordHasher<T>` for secrets

### SportsEComm.McpServer
- **Framework**: .NET 10 minimal API — HTTP only
- **Role**: tool proxy; maps `/tools/*` routes to backend API calls, forwards `Authorization` headers
- **Config**: `BackendApi:Url` in `appsettings.json` (default `http://localhost:5000`) or env var `BACKEND_API_URL`

### SportsEComm.Chatbot.Common
- **`ModelClient`** — posts to Ollama `/api/generate` (`stream: false`), reads `response` field
- **`McpClient`** — HTTP client for MCP tool endpoints; attaches JWT when provided
- **`ConversationManager`** — orchestration loop: prompt → detect `tool_call` JSON → MCP → second prompt → answer
- **`PromptTemplates`** — builds system + user prompts and tool-result follow-up prompts

### SportsEComm.Chatbot.Console
- Entry point; reads `appsettings.json` for `Model:Endpoint`, `Model:Name`, `MCP:BaseUrl`, `SystemPrompt`
- Handles `/login <email> <secret>` and `/exit` slash commands
- Delegates all other messages to `ConversationManager`

---

## Design decisions

| Decision | Rationale |
|----------|-----------|
| HTTP-only for local demo | Avoids dev-cert issues; `UseHttpsRedirection` is commented out but preserved |
| JWT identity from token claims | Customer ID is never trusted from URL parameters — extracted from `HttpContext.User` |
| `ReferenceHandler.IgnoreCycles` | EF navigation properties create circular refs (`Cart → CartItem → Cart`); ignored at serialization |
| `stream: false` on Ollama | Simplifies parsing; full text arrives in a single JSON object |

---

## Ports

| Service | Default URL |
|---------|------------|
| SportsEComm.Api | `http://localhost:5000` |
| SportsEComm.McpServer | `http://localhost:6000` |
| Ollama | `http://localhost:11434` |
