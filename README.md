# Event Management System

A full-featured, modular, and cleanly-architected ASP.NET Core solution for creating, managing, and attending events. It showcases modern .NET practices including Domain-Driven Design (DDD), CQRS with MediatR, Clean Architecture, SignalR, Azure Blob Storage integration, role-based security with JWT + cookies, caching, validation, and a rich test suite.

Contents
- Features
- Solution Architecture
- Tech Stack
- Getting Started
- Configuration
- Database and Migrations
- Run the API
- Authentication and Authorization
- Real-time Notifications (SignalR)
- Testing
- Docker / Containerization
- Code Style and Quality
- Contributing

---

## Features

- Event and category CRUD with capacity management
- User registration, authentication, refresh tokens (JWT stored in secure cookies or headers)
- Role-based authorization (Admin/User) with custom policies
- Event images: upload, set primary, delete (Azure Blob Storage)
- Real-time notifications via SignalR (email fallback)
- Dashboard/statistics endpoints (e.g., events nearing capacity)
- Caching layer to minimize DB round-trips
- Domain events and handlers (e.g., EventCapacityReachedEvent)
- Unit, integration, and SignalR tests

---

## Solution Architecture

Clean Architecture with strict separation of concerns:

- Core
  - Domain: Pure domain entities, value objects, and domain events
  - Application: Use cases (CQRS/MediatR), validators, behaviors, interfaces, models
- Infrastructure
  - Persistence: EF Core, repositories, Unit of Work, migrations, caching
  - Identity: ASP.NET Core Identity, JWT issuance, auth services
  - Utils: Cross-cutting concerns (SignalR hubs, background services, notifications)
- Presentation
  - EventManagementSystem.API: Minimal APIs, middleware, DI composition, Swagger, static files (SignalR test page)

Example structure (abridged):
- Core
  - Domain
    - Entities: Event, EventRegistration, EventImage, EventCategory, User
    - Events: EventCreatedEvent, EventCapacityUpdatedEvent, EventCapacityReachedEvent, RegistrationCancelledEvent, System events
  - Application
    - Usecases/Commands/...
    - Common/Behaviors: ValidationBehavior, PerformanceBehavior
    - Common/Interfaces: INotificationService, ISignalRNotificationService
    - Common/Models: Result, Error
    - EventHandlers: for domain events
- Infrastructure
  - Persistence
    - Context: ApplicationDbContext
    - Repositories: BaseRepository, EventRepository, EventRegistrationRepository
    - UoW: UnitOfWork
  - Identity
    - Entities: ApplicationUser, ApplicationRole, RefreshToken
    - Services: AuthenticationService, IJwtService
    - Configuration: JwtSettings
    - Context: IdentityDbContext (+ DesignTime factory)
  - Utils
    - Services: NotificationHub (SignalR), SignalRNotificationService, EnhancedNotificationService, NotificationBackgroundService
- Presentation
  - API
    - Endpoints: AuthenticationEndpoints, AdminEndpoints, etc.
    - Extensions: AuthenticationExtensions, SignalRExtensions, ServiceCollectionExtensions, ApplicationExtensions
    - Middleware: SecurityHeadersMiddleware, RequestLoggingMiddleware, GlobalExceptionMiddleware
    - Models: ApiResponse, PaginationParameters, RefreshTokenRequest
    - wwwroot/signalr-test.html

---

## Tech Stack

- .NET 9 (ASP.NET Core Minimal APIs)
- EF Core
- MediatR (CQRS, pipeline behaviors)
- ASP.NET Core Identity + JWT
- SignalR
- Azure Blob Storage (or Azurite for local dev)
- xUnit (tests), FluentValidation
- StyleCop + .editorconfig

---

## Getting Started

Prerequisites
- .NET 9 SDK
- A relational DB (SQL Server or PostgreSQL)
- Azure Storage Account or Azurite (for images)
- Node 18+ (only if you plan a SPA front-end)
- Visual Studio 2022 (latest) or VS Code

Clone and Restore
- If you already have the code locally, skip cloning.
- From the solution root:
  - dotnet restore

Trust HTTPS Dev Certificate
- dotnet dev-certs https --trust

Visual Studio notes
- Open the solution in Visual Studio 2022
- Set the API project as startup (__Set as Startup Project__)
- Run with __Start Debugging__ (F5). You can choose the debug target (__Debug Target__) between __IIS Express__ and __Project__.

---

## Configuration

Use appsettings.Development.json and/or User Secrets for local settings.

Required settings (sample):

{ "ConnectionStrings": { "DefaultConnection": "Server=localhost;Database=EventDb;User Id=sa;Password=Your_password123;" }, "JwtSettings": { "Issuer": "EventMS", "Audience": "EventMS", "Secret": "PLEASE_CHANGE_ME_TO_A_LONG_SECRET", "ExpiresInMinutes": 15 }, "AzureBlobStorage": { "ConnectionString": "UseDevelopmentStorage=true", "Container": "event-images" } }


Tips
- Use __Manage User Secrets__ in Visual Studio for secrets.
- For Azurite, set ConnectionString = "UseDevelopmentStorage=true".
- Ensure strong JWT Secret in production and store via your platform’s secret manager.

---

## Database and Migrations

Apply migrations for both Persistence (ApplicationDbContext) and Identity (IdentityDbContext).

CLI (from solution root):
- Application database
  - dotnet ef database update --project Infrastructure/Persistence/EventManagementSystem.Persistence
- Identity database
  - dotnet ef database update --project Infrastructure/Identity/EventManagementSystem.Identity

Add new migrations (examples):
- dotnet ef migrations add Init_App --project Infrastructure/Persistence/EventManagementSystem.Persistence
- dotnet ef migrations add Init_Identity --project Infrastructure/Identity/EventManagementSystem.Identity

On first run, the seeder creates default roles, an admin user, and sample data.

---

## Run the API

CLI
- dotnet run --project Presentation/EventManagementSystem.API

Swagger
- Swagger UI is enabled in Development. Check the console output for the actual port (commonly https://localhost:5001/swagger).

Static assets
- A SignalR test page is available at /signalr-test.html (served from wwwroot).

---

## Authentication and Authorization

- JWTs issued by Identity (IJwtService).
- Token is read from either:
  - HttpOnly, SameSite=Strict cookie named AccessToken, or
  - Authorization: Bearer <token> header (useful for Swagger/CLI).
- Policies:
  - RequireAdminRole → Admin role required
  - RequireUserRole → User or Admin
- Endpoints enforce policies via Minimal API configuration in the Endpoints folder.

Security notes
- Always use HTTPS.
- Rotate JWT secrets; use short-lived access tokens with refresh tokens.

---

## Real-time Notifications (SignalR)

- NotificationHub exposes real-time events (e.g., capacity updates).
- SignalRNotificationService broadcasts domain events to clients.
- A fallback EnhancedNotificationService/BackgroundService can send alternative notifications.
- Try the test page: https://localhost:<port>/signalr-test.html

---

## Testing

Run all tests:
- dotnet test

Notes
- Validation and behavior tests live under Application.
- SignalR integration tests are in Tests/SignalRIntegrationTests.cs.
- Consider running with a test DB and isolated storage.

---

## Docker / Containerization

Sample Dockerfile (multi-stage) targeting .NET 9:
