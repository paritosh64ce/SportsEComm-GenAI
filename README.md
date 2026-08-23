# Sports eCommerce GenAI Showcase 🏈⚽🏀

Welcome to the **Sports eCommerce GenAI Showcase** repository! This project demonstrates how Artificial Intelligence, through Large Language Models and the **Model Context Protocol (MCP)**, can seamlessly integrate with a modern enterprise `.NET 10`, **Entity Framework Core**, and **SQL Server** backend to deliver conversational shopping experiences.

---

## 🏗️ Architecture & Tech Stack

- **Backend API (`SportsEComm.Api/`)**: Built with **.NET 10 Web API** and **Entity Framework Core 10** connected to **SQL Server**. Exposes RESTful endpoints for products, customers, and orders.
- **MCP Server (`SportsEComm.McpServer/`)**: Implements the **Model Context Protocol**, exposing domain tools (`list_products`, `get_customer_orders`, `place_order`) to AI agents.
- **Chatbot Client (`SportsEComm.Chatbot/`)**: An AI-powered assistant that connects to the MCP server, enabling users to interact via natural language to browse sports gear, check order status, and place orders.

---

## 📂 Repository Structure

```text
SportsEComm-GenAI/
├── README.md               # You are here
├── docs/
│   └── implementation-plan.md # Detailed architecture & development plan
├── SportsEComm.Api/        # .NET 10 Web API, EF Core & SQL Server
├── SportsEComm.McpServer/  # Model Context Protocol server implementation
└── SportsEComm.Chatbot/    # AI Chatbot assistant project
```

---

## 🚀 Getting Started

Please refer to the [Implementation Plan](docs/implementation-plan.md) for detailed setup instructions, database migrations, and running the individual components.

### Running the services locally (ports)

- Backend API (SportsEComm.Api):
  - HTTP: http://localhost:5000
  - HTTPS: https://localhost:5001
  - Run: dotnet run --project SportsEComm.Api --urls "http://localhost:5000;https://localhost:5001"

- MCP Server (SportsEComm.McpServer):
  - HTTP: http://localhost:6000
  - Run: dotnet run --project SportsEComm.McpServer --urls "http://localhost:6000"

Notes:
- MCP server reads the backend API base URL from appsettings.json (BackendApi:Url) or the BACKEND_API_URL environment variable. By default the MCP server points to http://localhost:5000.
- Start the Backend API first so the MCP server can successfully proxy requests to it.



## TODO

- add proper jwt authentication, encrypt secret and store
- user should be identified from jwt token and instead of using customerId in api url parameter, user/customer should be identified using http context, so that user only access his/her data, instead of to be able to provide someone else customer id in url param
- add mcp server
- add chatbot