# API Reference

## Backend API — `http://localhost:5000`

### Public endpoints (no auth required)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/products` | List all products in the catalog |
| GET | `/api/products/{id}` | Fetch a single product by ID |
| GET | `/api/customers` | List all seeded customer accounts |
| POST | `/api/customers/login` | Authenticate and receive a JWT |

**Login request body**
```json
{
  "email": "ms.dhoni@teamindia2011.com",
  "secretKey": "dhoni7#cup"
}
```

**Login response**
```json
{
  "token": "<signed-jwt>",
  "customerId": 1,
  "name": "MS Dhoni"
}
```

---

### Protected endpoints (JWT required)

Pass the token as `Authorization: Bearer <token>`.  
Customer identity is resolved from the JWT claims — no customer ID in the URL.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/cart` | Fetch the authenticated customer''s cart |
| POST | `/api/cart` | Add or update a cart item |
| DELETE | `/api/cart/items/{productId}` | Remove a product from the cart |
| GET | `/api/orders` | List the authenticated customer''s orders |
| POST | `/api/orders` | Place an order from the current cart |

**Add to cart request body**
```json
{ "productId": 1, "quantity": 1 }
```

---

### API docs

Swagger UI is available at `http://localhost:5000/swagger` when running in Development mode.

---

## MCP Server — `http://localhost:6000`

The MCP server proxies calls to the backend API.  
All MCP endpoints accept `POST` (for tool invocations) or `GET` (for list-style reads).  
Protected tools forward the `Authorization: Bearer <token>` header from the request.

### Tool endpoints

| Method | Path | Maps to | Auth |
|--------|------|---------|------|
| GET | `/tools/list_products` | `GET /api/products` | No |
| POST | `/tools/search_products` | `GET /api/products` + filter | No |
| GET | `/tools/get_product` | `GET /api/products/{id}` | No |
| POST | `/tools/customer_login` | `POST /api/customers/login` | No |
| GET | `/tools/cart` | `GET /api/cart` | Yes |
| POST | `/tools/add_to_cart` | `POST /api/cart` | Yes |
| DELETE | `/tools/remove_from_cart/{productId}` | `DELETE /api/cart/items/{productId}` | Yes |
| GET | `/tools/orders` | `GET /api/orders` | Yes |
| POST | `/tools/place_order` | `POST /api/orders` | Yes |

### `search_products` filter body
```json
{
  "query": "bat",
  "maxPrice": 5000,
  "category": "cricket"
}
```
All fields are optional. Returns a filtered product list.

---

## Ollama — `http://localhost:11434`

The chatbot calls Ollama directly using the native generate API:

```
POST /api/generate
{
  "model": "mistral:7b",
  "prompt": "...",
  "stream": false
}
```

Response field used: `response` (string).
