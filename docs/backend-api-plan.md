# Backend API & Seed Data Implementation Plan

This document outlines the detailed technical design, database schema, Entity Framework Core models, SQL Server LocalDB connection strategy, and specific seed data for the **Sports eCommerce Backend API (`SportsEComm.Api/`)**.

---

## 1. Database & Technology Stack

- **Database Engine**: SQL Server LocalDB (`Server=(localdb)\\mssqllocaldb;Database=SportsECommDb;Trusted_Connection=True;MultipleActiveResultSets=true`)
- **ORM**: Entity Framework Core 10
- **Data Models**:
  - `Product`: Cricket equipment catalog.
  - `Customer`: Users based on the **2011 Cricket World Cup Indian Squad**, with 4 primary users enabled for chatbot login and order placement demonstrations. Includes hashed/encrypted secret API keys for secure authentication.
  - `Cart` & `CartItem`: Shopping cart management per customer.
  - `Order` & `OrderItem`: Transaction records representing customer purchases.

---

## 2. Seed Data Specifications

### A. Customers (2011 India World Cup Squad Highlights & Login Users)

To make demos engaging and secure, key members of the 2011 Indian World Cup squad are seeded. Four players are specifically configured for chatbot login using an **Email + Secret Key** mechanism. Secret keys are securely hashed/encrypted at rest in the database:

1. **MS Dhoni** (Captain & Wicketkeeper) *(Demo Login Enabled)*
   - Email: `ms.dhoni@teamindia2011.com`
   - Secret Key: `dhoni7#cup` (Stored as secure hash/encrypted format)
2. **Sachin Tendulkar** (Master Blaster) *(Demo Login Enabled)*
   - Email: `sachin.tendulkar@teamindia2011.com`
   - Secret Key: `sachin10#master` (Stored as secure hash/encrypted format)
3. **Virat Kohli** (Rising Star) *(Demo Login Enabled)*
   - Email: `virat.kohli@teamindia2011.com`
   - Secret Key: `kohli18#chase` (Stored as secure hash/encrypted format)
4. **Yuvraj Singh** (Player of the Tournament) *(Demo Login Enabled)*
   - Email: `yuvraj.singh@teamindia2011.com`
   - Secret Key: `yuvraj12#champ` (Stored as secure hash/encrypted format)
5. **Gautam Gambhir**, **Virender Sehwag**, **Zaheer Khan**, **Harbhajan Singh**, **Suresh Raina**, **Munaf Patel**, **S Sreesanth** *(Squad Members)*

---

### B. Products (Premium Cricket Equipment)

The product catalog features high-end cricket gear with rich descriptions, pricing, and stock levels:

1. **MSD Signature English Willow Cricket Bat**
   - *Description*: Grade 1+ seasoned English willow bat, massive sweet spot, favored by power hitters.
   - *Price*: ₹28,999.00
   - *Stock*: 15
2. **Master Blaster Pro Cricket Bat**
   - *Description*: Hand-crafted premium willow with exceptional balance and pickup for classical stroke play.
   - *Price*: ₹32,499.00
   - *Stock*: 10
3. **World Cup Edition Leather Cricket Ball (Pack of 6)**
   - *Description*: Four-piece alum tanned wax polished red leather balls designed for professional matches.
   - *Price*: ₹4,500.00
   - *Stock*: 50
4. **ProKeeper Cricket Wicketkeeping Gloves**
   - *Description*: Premium aniline leather palms with octopus rubber grip and high-density foam protection.
   - *Price*: ₹5,999.00
   - *Stock*: 25
5. **TurboFlex Full Cricket Batting Pads**
   - *Description*: Lightweight ultra-protective ergonomic leggards with high-density cane reinforcement.
   - *Price*: ₹6,499.00
   - *Stock*: 20
6. **StumpVision Pro Cricket Helmet**
   - *Description*: Titanium grill steel-reinforced helmet complying with latest safety British standards.
   - *Price*: ₹7,199.00
   - *Stock*: 18
7. **All-Rounder Heavy-Duty Cricket Kit Bag**
   - *Description*: Wheeled heavy-duty compartmentalized bag with shoe tunnel and thermal bat pocket.
   - *Price*: ₹8,999.00
   - *Stock*: 12

---

## 3. API Endpoints Design

- **Products**:
  - `GET /api/products`: List all available cricket equipment.
  - `GET /api/products/{id}`: Get product details.
- **Customers / Auth**:
  - `GET /api/customers`: List demo customers.
  - `POST /api/customers/login`: Authenticate customer using `{ "email": "...", "secretKey": "..." }`. Validates against encrypted/hashed keys stored in the database and returns a secure token or session ID.
- **Orders & Cart**:
  - `GET /api/cart?customerId={id}`: Retrieve customer shopping cart items.
  - `POST /api/cart`: Add or update item quantity in the cart.
  - `DELETE /api/cart/{customerId}/items/{productId}`: Remove item from cart.
  - `GET /api/orders?customerId={id}`: Retrieve all orders placed by a specific customer (used by the chatbot when logged in).
  - `POST /api/orders`: Place a new order from cart (or direct items) and clear the customer's cart upon successful placement.

---

## 4. EF Core Context & Migration Plan

1. Configure `DbContext` with DbSet properties for `Products`, `Customers`, `Carts`, `CartItems`, `Orders`, and `OrderItems`.
2. Implement `DbInitializer` extension method to seed the 2011 World Cup squad and cricket products on startup if the database is empty.
3. Use EF Core Migrations to generate schema for SQL Server LocalDB.
