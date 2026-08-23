# Sports eCommerce GenAI Showcase 🏈⚽🏀

This repository demonstrates a .NET 10 sports eCommerce solution that combines a backend API, an MCP-style proxy server, and a future-ready chatbot integration. The implemented backend currently supports product browsing, customer authentication, cart management, and protected order workflows using JWT-based authorization.

---

## Current implementation status

- ✅ **Backend API**: implemented in `SportsEComm.Api/`
- ✅ **MCP proxy server**: implemented in `SportsEComm.McpServer/`
- ⚠️ **Chatbot client**: scaffolded in `SportsEComm.Chatbot/` but not yet connected to an LLM or MCP flow

---

## Architecture & tech stack

- **Backend API (`SportsEComm.Api/`)**:
  - **Framework**: .NET 10 Web API
  - **Database / ORM**: SQL Server LocalDB via Entity Framework Core 10
  - **Features**: product catalog, customer login, JWT-authenticated cart, and order APIs

- **MCP Server (`SportsEComm.McpServer/`)**:
  - **Framework**: .NET 10 minimal HTTP proxy that exposes tool-style endpoints for the backend API
  - **Configuration**: reads `BackendApi:Url` from `appsettings.json` or the `BACKEND_API_URL` environment variable
  - **Default URLs**: backend `http://localhost:5000`, MCP server `http://localhost:6000`

- **Chatbot Project (`SportsEComm.Chatbot/`)**:
  - **Current state**: a minimal .NET 10 console scaffold (`Hello, World!`)
  - **Planned role**: connect to the MCP server and enable natural-language shopping flows

---

## 📂 Repository structure

```text
SportsEComm-GenAI/
├── README.md                       # Project overview and local run instructions
├── docs/
│   ├── implementation-plan.md      # Status + roadmap notes
│   ├── backend-api-plan.md         # Database and seed data plan/status
│   └── backend-api-technical-plan.md # API design and endpoint plan
├── SportsEComm.Api/               # .NET 10 Web API with EF Core and JWT auth
├── SportsEComm.McpServer/         # MCP-style tool proxy to the backend API
├── SportsEComm.Chatbot/           # Stub project for future LLM integration
├── SportsEComm-GenAI.slnx         # Solution file
└── LICENSE
```

---

## 🚀 Getting started

### Prerequisites

- .NET 10 SDK
- SQL Server LocalDB (the API uses `Server=(localdb)\mssqllocaldb;Database=SportsECommDb;Trusted_Connection=True;MultipleActiveResultSets=true` by default)

### Build the solution

```bash
dotnet build "SportsEComm-GenAI.slnx"
```

### Run the backend API

```bash
dotnet run --project SportsEComm.Api --urls "http://localhost:5000;https://localhost:5001"
```

### Run the MCP server

```bash
dotnet run --project SportsEComm.McpServer --urls "http://localhost:6000"
```

### Run the chatbot project

```bash
dotnet run --project SportsEComm.Chatbot
```

Current chatbot output is only a placeholder message and is not yet integrated with the backend or MCP server.

---

## Authenticated API flow

The backend currently follows a JWT-based flow:

- `POST /api/customers/login` with `{ "email": "...", "secretKey": "..." }`
  - Validates against demo customer credentials seeded in the database
  - Returns a signed JWT token
- Protected endpoints require the bearer token and resolve the customer ID from `HttpContext.User`

### Core protected endpoints

- `GET /api/cart`
- `POST /api/cart`
- `DELETE /api/cart/items/{productId}`
- `GET /api/orders`
- `POST /api/orders`

### Public endpoints

- `GET /api/products`
- `GET /api/products/{id}`
- `GET /api/customers`
- `POST /api/customers/login`

---

## Current gaps

- The chatbot project still needs a real AI orchestration layer and MCP client implementation.
- The MCP server is functional as a proxy, but the final conversational experience still needs to be implemented on top of it.
- The current backend intentionally uses JWT identity instead of customer ID route parameters for authorization.
