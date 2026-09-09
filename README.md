# Ndalama Kitchen

Internal kitchen tool for food orders (ASP.NET Core 8 + Angular 18).

## Structure (simple clean architecture)

**Backend**

| Folder | Role |
| --- | --- |
| `Controllers/` | HTTP API |
| `Models/` | Order, menu, status rules |
| `Dtos/` | API request/response shapes |
| `Services/` | Business logic |
| `Data/` | SQL Server LocalDB (EF Core) |

**Frontend:** `app/orders/` (list, form, detail) + `app/shared/`.

## Run

Backend (or open `backend/OrderTracker.sln` in Visual Studio → F5):

```
cd backend
dotnet test
dotnet run --project src/OrderTracker.Api
```

Frontend:

```
cd frontend
npm start
```

API http://localhost:5080 · UI http://localhost:4200

More detail: [SOLUTION.md](SOLUTION.md).
