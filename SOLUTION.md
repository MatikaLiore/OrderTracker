# Design notes

Small kitchen food-order app. Structure stays simple so it is easy to demo and explain.

## Backend layout

```
Controllers/   HTTP endpoints
Models/        Order, LineItem, MenuItem, OrderStatus, rules
Dtos/          Request/response shapes sent over the API
Services/      Business rules (OrderService)
Data/          Database (AppDbContext, OrderStore, menu seed)
```

| Name | Meaning |
| --- | --- |
| **Dtos** | Data Transfer Objects — JSON bodies for the API (not domain models) |
| **Data** | EF Core + LocalDB access |
| **OrderStore** | Thin helper that reads/writes the database |
| **OrderService** | Validates, places orders, calculates totals, changes status |

Money rounding lives as a private `RoundMoney` method inside `OrderService` — not a separate class.

Validation uses **DataAnnotations** on the DTOs (`[Required]`, `[MaxLength]`, `[Range]`, `[MinLength]`) so ASP.NET returns 400 automatically. `OrderService` still checks business rules (menu item exists, max lines) and the same basics when called from unit tests.

## Frontend layout

```
app/
  shared/          api-error helper
  orders/          feature folder
    order.model.ts
    order-api.service.ts
    orders.routes.ts
    order-list/
    order-form/
    order-detail/
```

Standard for a small Angular app: one feature folder, each screen in its own folder, lazy-loaded routes.

## Flow

`Pending` → `Confirmed` → `Ready` → `Done` (+ Cancel from Pending/Confirmed).

Order numbers: `ORD-0001`. Menu prices come from the database; totals are calculated on the server.
