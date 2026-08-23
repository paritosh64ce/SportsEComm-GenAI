# Backend API Technical & Architecture Plan

This document reflects the current implementation in `SportsEComm.Api/` and highlights the design patterns that are in place today. The project follows a layered service/repository structure, applies JWT-based authorization, and uses EF Core to persist the sports catalog and customer data.

---

## 1. Current architecture snapshot

```text
SportsEComm.Api/
├── Controllers/          # REST endpoints (presentation layer)
├── Data/                 # DbContext + startup seeding logic
├── DTOs/                 # Request/response payloads
├── Extensions/           # Swagger and app setup helpers
├── Models/               # Domain entities
├── Repositories/         # Generic repository + unit of work
├── Services/             # Business logic and auth helpers
└── Program.cs            # Service registration and middleware setup
```

### Design patterns currently used

1. **Dependency injection**
   - Services, repository abstractions, and EF Core context are registered in `Program.cs`.
2. **Repository + Unit of Work**
   - `IRepository<T>` and `IUnitOfWork` abstract the data access layer.
3. **Service layer separation**
   - Controllers delegate business logic to `ProductService`, `CustomerService`, `CartService`, and `OrderService`.
4. **JWT authentication**
   - `[Authorize]` routes read the customer identity from the token claims instead of trusting a route parameter.

---

## 2. Authentication & authorization strategy

- **Secure credential authentication**: `POST /api/customers/login` accepts an email and secret key.
- **Hashed storage at rest**: secret values are stored using ASP.NET Core `PasswordHasher<T>` in the `Customer.SecretKeyHash` field.
- **JWT-based identity**: successful login returns a signed JWT containing customer claims such as `NameIdentifier`, `Email`, `Name`, and `Role`.
- **Protected endpoint pattern**: all cart/order routes require the bearer token and resolve the customer ID from `HttpContext.User`.

This prevents customer IDs from being passed in URL parameters and ensures each authenticated user only operates on their own cart and orders.

---

## 3. Core interfaces & classes

### A. Generic repository interface

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

### B. Unit of work interface

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

### C. Business services

- `IProductService`: list and lookup products
- `ICustomerService`: authenticate users and return login responses
- `ICartService`: manage customer-specific cart items
- `IOrderService`: create and fetch orders based on the authenticated customer
- `IAuthService`: hash secrets and generate JWT tokens

---

## 4. REST API endpoints specification

| Method | Endpoint | Description | Notes |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/products` | Return all available cricket products | Public |
| **GET** | `/api/products/{id}` | Return a product by ID | Public |
| **GET** | `/api/customers` | Return seeded customer list | Public |
| **POST** | `/api/customers/login` | Authenticate using email + secret key | Returns JWT |
| **GET** | `/api/cart` | Return the current customer’s cart | Requires JWT |
| **POST** | `/api/cart` | Add or update a cart item | Requires JWT |
| **DELETE** | `/api/cart/items/{productId}` | Remove a cart item | Requires JWT |
| **GET** | `/api/orders` | Return orders for the authenticated customer | Requires JWT |
| **POST** | `/api/orders` | Place a new order from the authenticated cart | Requires JWT |

Important: the protected order and cart routes do not use a customer ID URL parameter. The customer identity is read from the JWT claim set in `HttpContext.User`.

---

## 5. API documentation with Swagger / OpenAPI

- Swagger is enabled in development mode through `AddSwaggerWithJwt()`.
- The API exposes interactive documentation at `/swagger` when the app is running in Development.
- This includes customer login, product browsing, shopping cart management, and order placement endpoints.
