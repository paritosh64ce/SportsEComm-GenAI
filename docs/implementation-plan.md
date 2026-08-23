# Sports eCommerce GenAI Showcase - Implementation Plan

This project showcases AI-driven capabilities within a modern sports eCommerce platform. It demonstrates how Large Language Models (LLMs) can interact via a Model Context Protocol (MCP) server with a robust .NET 10 backend API and SQL Server database to manage products, view customer orders, and place new orders conversationally.

---

## 1. Architecture & Tech Stack

- **Backend API (`SportsEComm.Api/`)**:
  - **Framework**: .NET 10 Web API
  - **Database / ORM**: SQL Server, Entity Framework Core 10
  - **Features**: Product catalog management, customer order management, and secure order placement endpoints.

- **MCP Server (`SportsEComm.McpServer/`)**:
  - **Framework**: .NET 10 MCP server that exposes domain "tools" (HTTP endpoints) for the AI agent and proxies calls to the backend API.
  - **Tools / Endpoints Exposed (example)**:
    - `GET /tools/list_products` -> proxies to `GET /api/products` on the backend
    - `GET /tools/get_customer_orders/{customerId}` -> proxies to `GET /api/orders/{customerId}`
    - `POST /tools/place_order` -> proxies to `POST /api/orders`
    - `POST /tools/customer_login` -> proxies to `POST /api/customers/login` (returns JWT token)
    - `GET /tools/cart` -> proxies to `GET /api/cart` (forwards Authorization header)
    - `POST /tools/cart` -> proxies to `POST /api/cart` (forwards Authorization header)
    - `DELETE /tools/cart/items/{productId}` -> proxies to `DELETE /api/cart/items/{productId}` (forwards Authorization header)
  - **Configuration**: Backend API base URL is configured with the environment variable BACKEND_API_URL (default: http://localhost:5000). The MCP server runs by default at http://localhost:6000 and forwards Authorization headers for protected endpoints. It can later be extended to manage sessions, token caching, or to integrate directly with an LLM runtime via MCP transports (STDIO, SSE) or tool registration.

- **Chatbot Project (`SportsEComm.Chatbot/`)**:
  - **Framework**: .NET 10 / Console or Web-based AI assistant client.
  - **Features**: Integrates with an LLM (e.g., Semantic Kernel or OpenAI client) and connects to the MCP server to execute natural language prompts for shopping and order tracking.

---

## 2. Project Structure

```text
SportsEComm-GenAI/
├── README.md               # Root overview and quickstart guide
├── docs/
│   └── implementation-plan.md # This detailed plan
├── SportsEComm.Api/        # .NET 10 Web API with EF Core & SQL Server
├── SportsEComm.McpServer/  # Model Context Protocol server exposing eCommerce tools
└── SportsEComm.Chatbot/    # AI Chatbot client interacting with the MCP server
```

---

## 3. Implementation Steps

1. **Root README & Documentation**:
   - Create root `README.md` explaining the project architecture, prerequisites, and setup instructions.
2. **Backend API (`backend-api`)**:
   - Initialize .NET 10 Web API project.
   - Configure Entity Framework Core with SQL Server (`SportsECommContext`).
   - Define Models: `Product`, `Order`, `OrderItem`, `Customer`.
   - Implement Controllers/Endpoints:
     - `GET /api/products`
     - `GET /api/orders/{customerId}`
     - `POST /api/orders`
3. **MCP Server (`mcp-server`)**:
   - Create MCP Server project to expose domain tools as HTTP endpoints that proxy to the backend API (examples: `GET /tools/list_products`, `GET /tools/get_customer_orders/{customerId}`, `POST /tools/place_order`).
   - Configure the backend URL via environment variable BACKEND_API_URL (default: http://localhost:5000).  
   - Later: extend to support MCP transports (STDIO, SSE) or register tools directly with an LLM runtime as needed.
4. **Chatbot (`chatbot`)**:
   - Implement AI client capable of tool calling via the MCP server.
   - Provide interactive chat loop for users to chat, browse products, view orders, and place orders.
