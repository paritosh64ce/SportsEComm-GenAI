# Backend API & Seed Data Implementation Status

This document captures the current implementation state of the sports eCommerce backend and the seed dataset used for demos and local development.

---

## 1. Database & technology stack

- **Database engine**: SQL Server LocalDB
- **Connection string**: `Server=(localdb)\mssqllocaldb;Database=SportsECommDb;Trusted_Connection=True;MultipleActiveResultSets=true`
- **ORM**: Entity Framework Core 10
- **Model coverage**: `Product`, `Customer`, `Cart`, `CartItem`, `Order`, and `OrderItem`
- **Startup behavior**: `DbInitializer.InitializeAsync` creates the database and seeds it if needed

---

## 2. Seed data specification

### A. Customers and demo login users

The seed data includes the 2011 Indian World Cup squad and several demo users for login-driven product and order flows. Secret values are not stored as plain text; they are hashed using ASP.NET Core `PasswordHasher<T>` before persistence.

1. **MS Dhoni** - `ms.dhoni@teamindia2011.com` / `dhoni7#cup`
2. **Sachin Tendulkar** - `sachin.tendulkar@teamindia2011.com` / `sachin10#master`
3. **Virat Kohli** - `virat.kohli@teamindia2011.com` / `kohli18#chase`
4. **Yuvraj Singh** - `yuvraj.singh@teamindia2011.com` / `yuvraj12#champ`
5. Additional squad members such as Gautam Gambhir, Virender Sehwag, Zaheer Khan, Harbhajan Singh, Suresh Raina, Munaf Patel, and S Sreesanth are also seeded.

### B. Products

The product catalog is seeded with premium cricket equipment and includes product pricing, stock counts, and image path values.

1. MSD Signature English Willow Cricket Bat
2. Master Blaster Pro Cricket Bat
3. World Cup Edition Leather Cricket Ball (Pack of 6)
4. ProKeeper Cricket Wicketkeeping Gloves
5. TurboFlex Full Cricket Batting Pads
6. StumpVision Pro Cricket Helmet
7. All-Rounder Heavy-Duty Cricket Kit Bag

---

## 3. API endpoints design

### Public endpoints

- `GET /api/products`: list all available products
- `GET /api/products/{id}`: fetch a single product by ID
- `GET /api/customers`: list the seeded customer accounts
- `POST /api/customers/login`: authenticate a customer and return a JWT

### Protected endpoints

- `GET /api/cart`: fetch the authenticated customer’s cart
- `POST /api/cart`: add or update a cart item
- `DELETE /api/cart/items/{productId}`: remove a cart item
- `GET /api/orders`: fetch the authenticated customer’s orders
- `POST /api/orders`: place an order from the authenticated customer’s cart

The backend resolves customer identity from JWT claims rather than route parameters. This matches the current secure authorization model in the app.

---

## 4. EF Core context & migration plan

1. The `SportsECommContext` exposes `Products`, `Customers`, `Carts`, `CartItems`, `Orders`, and `OrderItems`.
2. The app seeds the database automatically on startup by calling `DbInitializer.InitializeAsync`.
3. Decimal values for `Order.TotalAmount`, `OrderItem.UnitPrice`, and `Product.Price` are configured with SQL Server decimal precision.
4. Local database initialization is dependency-free for the current solution and is suitable for demo and development use.
