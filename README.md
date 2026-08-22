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


## TODO

- add proper jwt authentication, encrypt secret and store
- add mcp server
- add chatbot