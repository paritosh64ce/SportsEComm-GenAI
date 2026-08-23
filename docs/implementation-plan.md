# Sports eCommerce GenAI Showcase - Implementation Status & Plan

This document records the actual repository state and the remaining work for the sports eCommerce GenAI solution. The backend API and MCP proxy are implemented, while the chatbot layer is still a placeholder for future LLM-driven interactions.

---

## 1. Current project state

- ✅ **Backend API**: implemented in `SportsEComm.Api/`
- ✅ **MCP server**: implemented in `SportsEComm.McpServer/`
- ⚠️ **Chatbot**: scaffolded in `SportsEComm.Chatbot/`, but not yet connected to any LLM or MCP tool-calling flow

---

## 2. Architecture & tech stack

- **Backend API (`SportsEComm.Api/`)**:
  - **Framework**: .NET 10 Web API
  - **Database / ORM**: SQL Server LocalDB, Entity Framework Core 10
  - **Features**: product catalog, customer authentication, JWT-authorized cart, and order endpoints

- **MCP Server (`SportsEComm.McpServer/`)**:
  - **Framework**: .NET 10 HTTP tool proxy
  - **Behavior**: proxies requests to the API and forwards Authorization headers for protected routes
  - **Configuration**: reads `BackendApi:Url` from `appsettings.json` or `BACKEND_API_URL`

- **Chatbot Project (`SportsEComm.Chatbot/`)**:
  - **Current state**: console skeleton only
  - **Next step**: add an AI runtime or MCP client layer and conversational orchestration

---

## 3. Completed work

1. **Root documentation and setup guidance**
   - Created project README with local run instructions and current status.
2. **Backend API implementation**
   - Added a .NET 10 Web API with controllers, DTOs, services, repositories, and EF Core models.
   - Configured JWT authentication and secure secret hashing with ASP.NET Core `PasswordHasher<T>`.
   - Added LocalDB-based seeding for products and demo customers.
3. **MCP proxy implementation**
   - Exposed HTTP endpoints such as `/tools/list_products`, `/tools/customer_login`, `/tools/cart`, and `/tools/place_order`.
   - Forwarded Authorization headers for protected backend calls.
4. **Database bootstrap**
   - `DbInitializer.InitializeAsync` ensures the database is created and seeded on app startup.

---

## 4. Remaining work

1. **Chatbot integration**
   - Add an LLM client or Semantic Kernel integration.
   - Implement tool-calling and conversational loops against the MCP server.
2. **MCP UX hardening**
   - Align the MCP routes with the final backend contract for order retrieval and cart actions.
   - Add richer validation and safer error propagation.
3. **Production hardening**
   - Move JWT secrets and connection settings to secure configuration or secret stores.
   - Evaluate deployment configuration for SQL Server, HTTPS, and environment-based settings.

---

## 5. Current execution order

1. Start the backend API on `http://localhost:5000`.
2. Start the MCP server on `http://localhost:6000`.
3. Use the backend for product and auth flows.
4. Extend the chatbot project to call the MCP endpoints for shopping and order actions.
