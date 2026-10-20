## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Architecture** | Clean Architecture (Domain, Application, Infrastructure, Api) |
| **Runtime** | .NET 10 / C# 14 |
| **API** | Minimal APIs with TypedResults |
| **CQRS** | MediatR |
| **Validation** | FluentValidation 12 + Result pattern |
| **Errors** | ProblemDetails (RFC 9457) + global exception handler |
| **Database** | EF Core 10 + PostgreSQL |
| **Caching** | Microsoft HybridCache (L1 in-memory + L2 Redis) |
| **Auth** | ASP.NET Identity + JWT with refresh tokens |
| **API Docs** | Scalar (modern OpenAPI UI) |
| **Logging** | Serilog 10 structured logging |
| **Observability** | .NET Aspire 13 + OpenTelemetry (traces, metrics, logs) |
| **Testing** | xUnit v3 + FluentAssertions + NSubstitute + NetArchTest |
| **Solution** | `.slnx` format + Central Package Management |

## Architecture

```
┌──────────────────────────────────────────────────┐
│                    Api Layer                     │
│         Endpoints, Program.cs, Scalar            │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│              Infrastructure Layer                │
│     EF Core, Identity, JWT, HybridCache          │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│              Application Layer                   │
│      CQRS Handlers, Validators, DTOs             │
└──────────────────┬───────────────────────────────┘
                   │ depends on
┌──────────────────▼───────────────────────────────┐
│                Domain Layer                      │
│              Entities, Result                    │
└──────────────────────────────────────────────────┘
```

## Project Structure

```
├── src/
│   ├── Application/       # CQRS commands/queries, handlers, validators
│   ├── Infrastructure/    # EF Core, Identity, JWT, caching, repositories
│   ├── Domain/           # Entities, value objects, repository interfaces
│   ├── Api/               # Minimal API endpoints, Scalar, middleware
│   ├── AppHost/           # Aspire orchestration (PostgreSQL + Redis)
│   └── ServiceDefaults/   # OpenTelemetry, health checks, resilience
├── tests/
│   ├── Architecture.Tests/ # Dependency rule enforcement (9 tests)
│   └── Application.UnitTests/ # Handler unit tests (8 tests)
├── Directory.Build.props     # .NET 10, C# latest, nullable enabled
├── Directory.Packages.props  # Central Package Management
├── .editorconfig             # Code style rules
├── compose.yaml        # standalone, no Aspire Dashboard
└── README.md
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Aspire containers)

### Run with Aspire (recommended)

```bash
cd src/AppHost
dotnet run
```

This starts everything:
- **PostgreSQL** database with pgAdmin
- **Redis** cache with RedisInsight
- **API** with auto-migration and seed data
- **Aspire Dashboard** for OpenTelemetry (traces, metrics, logs)

Open the Aspire Dashboard URL from the console output to see your telemetry.

### Run without Aspire

```bash
cp .env.example .env
docker compose up --build 
```
If you run the application using only Docker Compose, you won't have access to the .NET Aspire Dashboard and its metrics.

### Explore the API

Navigate to `https://localhost:7200/scalar/v1` for the interactive Scalar API docs.

**Default admin credentials** (seeded automatically):
- Email: `admin@example.dev`
- Password: `Admin123!`

### Run Tests

```bash
cd src
dotnet test CheckInNow.slnx
```

## Sample: Todos Feature

The template includes a complete **Todos** CRUD feature as a reference implementation:

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/todos` | GET | Yes | Get all todos (paginated) |
| `/api/todos/{id}` | GET | Yes | Get a todo by ID |
| `/api/todos` | POST | Yes | Create a new todo |
| `/api/todos/{id}` | PUT | Yes | Update a todo |
| `/api/todos/{id}/complete` | PATCH | Yes | Mark as completed |
| `/api/todos/{id}` | DELETE | Yes | Delete a todo |

### Adding a New Feature

Follow the Todos pattern:

1. **Domain** — Add your entity in `Domain/Entities/`
2. **Application** — Create a feature folder in `Application/Features/YourFeature/` with command/query/event records, command/event handlers, and validators
3. **Infrastructure** — Add EF Core configuration in `Infrastructure/Persistence/Configurations/`
4. **Api** — Add endpoints in `Api/Endpoints/` and register in `Program.cs`