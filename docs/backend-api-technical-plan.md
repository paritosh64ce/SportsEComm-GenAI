# Backend API Technical & Architecture Plan

This document outlines the software design and architectural patterns for implementing the **Sports eCommerce Backend API (`backend-api/`)**. The design strictly adheres to **SOLID principles**, **Clean Architecture tenets**, **Generic Repository & Unit of Work patterns**, **Dependency Injection**, and **RESTful API design**.

---

## 1. Architecture & Design Patterns

To ensure high maintainability, testability, and extensibility, the backend API is structured into distinct architectural layers:

```text
backend-api/
├── Controllers/          # REST endpoints (Presentation Layer)
├── Data/                 # EF Core DbContext & Initializer (Infrastructure)
├── Models/               # Domain Entities (Products, Customers, Orders)
├── Repositories/         # Generic Repository & Unit of Work (Data Access Layer)
└── Services/             # Business Logic Layer (Domain Services)
```

### Key Patterns & Principles:
1. **SOLID Principles**:
   - **Single Responsibility (SRP)**: Controllers handle HTTP routing; Services handle business logic; Repositories handle data access.
   - **Open/Closed (OCP)**: Interfaces (`IRepository<T>`, `IUnitOfWork`, `IOrderService`, etc.) allow new implementations or decorators without modifying existing core classes.
   - **Dependency Inversion (DIP)**: High-level services depend on abstractions (`IRepository<T>`), wired up via built-in .NET Dependency Injection (`IServiceCollection`).
2. **Generic Repository Pattern**:
   - A generic interface `IRepository<T>` and implementation `Repository<T>` encapsulating standard CRUD operations (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Delete`).
3. **Unit of Work Pattern**:
   - `IUnitOfWork` coordinates writes across multiple repositories and manages transactions and `SaveChanges` atomically.

---

## 2. Core Authentication & Authorization Strategy

- **Secure Credential Authentication**: Customers authenticate via `POST /api/customers/login` providing both their registered Email and Secret Key.
- **Encrypted Storage at Rest**: Secret keys are stored in the database using strong hashing (e.g., ASP.NET Core PasswordHasher / BCrypt) or encrypted representation.
- **Authorization Guard**: Upon successful login, the API issues a session token/ID passed via headers (`Authorization: Bearer <token>` or `X-Customer-Token`). Cart and Order endpoints (`/api/cart`, `/api/orders`) enforce strict validation of this token.

---

## 3. Core Interfaces & Classes Design

### A. Generic Repository Interface
```csharp
namespace SportsEComm.Api.Repositories;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
```

### B. Unit of Work Interface
```csharp
namespace SportsEComm.Api.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Product> Products { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Cart> Carts { get; }
    IRepository<Order> Orders { get; }
    Task<int> CompleteAsync();
}
```

### C. Business Services
- `IProductService`: Manages catalog queries and stock checks.
- `ICustomerService`: Handles customer authentication (e.g. MS Dhoni, Sachin Tendulkar, Virat Kohli, Yuvraj Singh login) and profile lookups.
- `IOrderService`: Validates order items, calculates totals, updates inventory, and persists orders via Unit of Work.

---

## 3. REST API Endpoints Specification

| Method | Endpoint | Description | Request Payload / Params |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/products` | Retrieve all cricket products | None |
| **GET** | `/api/products/{id}` | Get product details by ID | Route ID |
| **GET** | `/api/customers` | List all 2011 WC squad customers | None |
| **POST** | `/api/customers/login` | Authenticate by email & secret key | `{ "email": "ms.dhoni@teamindia2011.com", "secretKey": "dhoni7#cup" }` |
| **GET** | `/api/cart` | Get customer shopping cart | Query `?customerId={id}` |
| **POST** | `/api/cart` | Add or update item in cart | `{ "customerId": 1, "productId": 2, "quantity": 2 }` |
| **DELETE** | `/api/cart/{customerId}/items/{productId}` | Remove item from cart | Route params |
| **GET** | `/api/orders` | Get orders for a customer | Query `?customerId={id}` |
| **POST** | `/api/orders` | Place order (clears cart) | `{ "customerId": 1 }` or explicit items |

---

## 5. API Documentation with Swagger / OpenAPI

- **OpenAPI Integration**: Integrated via Swashbuckle / Microsoft OpenAPI packages.
- **Interactive UI**: Available at root (`/swagger`) when running in Development mode, enabling full exploration of Products, Cart, Orders, and Customer login endpoints.
