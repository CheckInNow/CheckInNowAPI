# CheckInNow API

Backend for a conference platform. Organizers create conferences, set up rooms and speakers and build the agenda; attendees will register and check in. Registrations, check-in, ratings and statistics are planned next.

## What it does

- **Accounts:** registration with email confirmation, login, Google login, JWT with refresh tokens. Roles: `User`, `Organizer`, `Admin`.
- **Conferences:** organizers create and manage their own conferences (soft delete). Listing and details are public.
- **Rooms and speakers:** every conference has its own rooms and speakers.
- **Sessions (agenda):** a session is one talk with a room, a speaker and a time slot. It must be within the conference dates, the room and the speaker must belong to the same conference, and sessions cannot overlap in the same room or for the same speaker. The room overlap rule is also enforced by a PostgreSQL exclusion constraint.

## Tech stack

.NET 10 with Minimal APIs, Clean Architecture (Domain, Application, Infrastructure, Api), CQRS with MediatR, FluentValidation and a `Result` type for errors, EF Core with PostgreSQL, HybridCache with Redis, ASP.NET Identity with JWT, Scalar for API docs, Serilog, .NET Aspire with OpenTelemetry, xUnit v3 with FluentAssertions, NSubstitute and NetArchTest. Package versions are managed centrally in `Directory.Packages.props`.

Architecture decisions are described in [docs/adr](docs/adr), the domain model in [docs/event-storming](docs/event-storming).

## Getting started

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and [Docker](https://www.docker.com/products/docker-desktop/).

### Run with Aspire (recommended)

```bash
cd src/AppHost
dotnet run
```

This starts PostgreSQL (with pgAdmin), Redis (with RedisInsight), Mailpit for emails and the API. The API applies migrations and seeds sample data on startup. The Aspire Dashboard URL is printed in the console; it shows logs, traces and metrics.

AppHost needs two parameters, `jwt-secret` and `google-client-id`. The dashboard asks for them on the first run, or you can set them with `dotnet user-secrets` in `src/AppHost` (`Parameters:jwt-secret`, `Parameters:google-client-id`).

### Run with Docker Compose

```bash
cp .env.example .env
docker compose up --build
```

Fill in `.env` first (database credentials, `Jwt__Secret`, `Authentication__Google__ClientId`). The API is available at `http://localhost:8080` and emails can be read in Mailpit on the port set in `MAILPIT_PORT`. This setup has no Aspire Dashboard.

### API docs

Scalar is available at `https://localhost:7200/scalar/v1` with Aspire and at `http://localhost:8080/scalar/v1` with Docker Compose.

### Seeded accounts

- `admin@example.dev` / `Admin123!` (Admin)
- `organizer@example.dev` / `Organizer123!` (Organizer)
- `organizer2@example.dev` / `Organizer123!` (Organizer)

The seeder also adds sample conferences with rooms, speakers and sessions.

## Tests

```bash
cd src
dotnet test CheckInNow.slnx
```

Unit tests cover the handlers and validators, architecture tests check the dependency rules between layers.
