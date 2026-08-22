# Sports eCommerce GenAI Showcase - Implementation Plan

This project showcases AI-driven capabilities within a modern sports eCommerce platform. It demonstrates how Large Language Models (LLMs) can interact via a Model Context Protocol (MCP) server with a robust .NET 10 backend API and SQL Server database to manage products, view customer orders, and place new orders conversationally.

---

## 1. Architecture & Tech Stack

- **Backend API (`SportsEComm.Api/`)**:
  - **Framework**: .NET 10 Web API
  - **Database / ORM**: SQL Server, Entity Framework Core 10
  - **Features**: Product catalog management, customer order management, and secure order placement endpoints.

- **MCP Server (`SportsEComm.McpServer/`)**:
  - **Framework**: .NET 10 / Node.js MCP server implementation exposing tools for the AI agent.
  - **Tools Exposed**:
    - `list_products`: Search or list available sports merchandise, equipment, and apparel.
    - `get_customer_orders`: Retrieve past and current orders for a given customer.
    - `place_order`: Create a new order for specified products and quantities.

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
   - Create MCP Server project to wrap backend API capabilities into tools (`list_products`, `get_customer_orders`, `place_order`).
   - Configure transport (Stdio / SSE) for LLM communication.
4. **Chatbot (`chatbot`)**:
   - Implement AI client capable of tool calling via the MCP server.
   - Provide interactive chat loop for users to chat, browse products, view orders, and place orders.
