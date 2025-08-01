# Event Management System 🗓️

A full-featured, **modular and cleanly-architected** ASP.NET Core solution for creating, managing and attending events.  
It showcases modern .NET practices such as **DDD, CQRS/MediatR, clean architecture, SignalR, Azure Blob Storage** integration, role–based security with **JWT + cookies**, caching, validation and a rich test-suite.

---

## ✨ Features

* Event / category CRUD with capacity management  
* User registration, authentication & refresh-tokens (JWT stored in secure cookies or headers)  
* Role-based authorization (`Admin` / `User`) with custom policies  
* Upload, set primary & delete event images (stored in Azure Blob Storage)  
* Real-time notifications via SignalR (and fall-back e-mail)  
* Dashboard & statistics end-points (events nearing capacity, upcoming events, etc.)  
* Caching layer to minimise DB round-trips  
* Comprehensive domain events (e.g. `EventCapacityReachedEvent`) and handlers  
* Integration, unit and SignalR tests

---

## 🏗️ Solution Structure

EventManagementSystem.sln
|-- Core
|-- Domain // Pure domain entities, VOs & domain events
-- Infrastructure
|-- Persistence // EF Core, repositories, migrations, caching
|-- Identity // ASP.NET Core Identity + JWT, auth services
|-- Utils // Cross-cutting concerns (SignalR hubs, notifications)
-- Presentation
|-- EventManagementSystem.API // Minimal-API endpoints & middleware

The architecture follows **Clean Architecture** / **DDD** principles:  
*Core* stays free of external concerns, *Infrastructure* provides implementations, *Presentation* wires everything together.

---

## 🔐 Authentication & Authorization

1. **JWT**s are issued by the Identity project (`IJwtService`) and stored in an `HttpOnly`, `SameSite=Strict` cookie named `AccessToken`.  
2. `AuthenticationExtensions` configures ASP.NET to **read the token from either the cookie or the `Authorization: Bearer` header**, so Swagger / CLI tools also work.
3. Two policies are registered:
   * `RequireAdminRole` → Must have `Admin`
   * `RequireUserRole`  → Must have `User` **or** `Admin`
4. End-points enforce them with `.RequireAuthorization("RequireAdminRole")` in the `Endpoints/` files.

---

## ⚙️ Getting Started

### Prerequisites
* .NET 8 SDK (or 7 – check your `*.csproj` `TargetFramework`)
* A relational DB (SQL Server / PostgreSQL) – configurable via connection string
* Azure Storage Account for images (or use Azurite emulator)
* Node 18+ (only if you serve a SPA from the same solution)

### Clone & Restore

```bash
git clone https://github.com/<your-org>/EventManagementSystem.git
cd EventManagementSystem
dotnet restore
```

### Environment variables

Create a **local `.env` / `User Secrets`** or update `appsettings.Development.json`.

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EventDb;User Id=sa;Password=Your_password123;"
  },
  "JwtSettings": {
    "Issuer": "EventMS",
    "Audience": "EventMS",
    "Secret":  "PLEASE_CHANGE_ME_TO_A_LONG_SECRET",
    "ExpiresInMinutes": 15
  },
  "AzureBlobStorage": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "Container": "event-images"
  }
}
```

### Database – run migrations & seed

```bash
dotnet ef database update --project Infrastructure/Persistence/EventManagementSystem.Persistence
```

(The `DatabaseSeeder` will populate default roles, admin user and sample data on first run.)

### Run the API

```bash
dotnet run --project Presentation/EventManagementSystem.API
# Swagger UI → https://localhost:5001/swagger
```

---

## 🧪 Testing

```bash
dotnet test
```

SignalR integration tests reside in `Tests/SignalRIntegrationTests.cs`.

---

## 🚀 CI / CD & Deployment

The solution is container-ready. A sample Dockerfile (multistage build) and `docker-compose.yml` can be added:

```dockerfile
# --- Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Presentation/EventManagementSystem.API -c Release -o /app/publish

# --- Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventManagementSystem.API.dll"]
```

Environment vars (connection strings, JWT secret, blob storage) are injected via your orchestration platform (Kubernetes, Azure App Service, etc.).

---

## 📈 Roadmap / Ideas

* Outlook / Google Calendar integration  
* Web-hooks for external systems  
* Admin UI (React or Next.js)  
* Payment gateway for paid events  
* Kubernetes Helm chart & GitHub Actions pipeline  

---

## 🤝 Contributing

1. Fork the repo & create your feature branch (`git checkout -b feature/amazing-thing`)
2. Commit your changes (`git commit -m 'feat: add amazing thing'`)
3. Push to the branch (`git push origin feature/amazing-thing`)
4. Open a Pull Request

Please make sure to run `dotnet format` and that all tests pass before submitting.

Happy hacking! 🎉