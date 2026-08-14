# EnsyInc.Enclave

[![Quality gate status](https://sonarcloud.io/api/project_badges/measure?project=EnsyInc_Enclave&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=EnsyInc_Enclave)

Backend for the **Licensing** service of EnsyInc's Licensing + Monitoring platform: a backoffice for granting, tracking, and renewing product licenses per customer organization. It's one of two deliberately separate services (the other being Monitoring) that share the concept of a Product but are isolated from each other so an incident in one doesn't take down the other. See [docs/00_overview.md](docs/00_overview.md) for the full platform rationale, [docs/licensing-service.md](docs/licensing-service.md) for the domain requirements, [docs/features.md](docs/features.md) for the feature/endpoint inventory, and [docs/models.md](docs/models.md) for entity and state diagrams. The frontend lives in [EnsyInc/Enclave.Web](https://github.com/EnsyInc/Enclave.Web).

## Status

Licensing is implemented end-to-end: `Product`, `Org`, `User`, `License`, and `License Request` all exist as domain models, EF Core entities/migrations, repositories, services, and API controllers (see [docs/features.md](docs/features.md)). `Monitoring` is designed (see the docs above) but not yet built.

## Architecture

A layered solution under `src/EnsyInc.Enclave/`:

| Project | Responsibility |
|---|---|
| `EnsyInc.Enclave.Api` | ASP.NET Core controllers, request/response models, validation, exception handling |
| `EnsyInc.Enclave.Services` | Application/domain logic, orchestrating repositories |
| `EnsyInc.Enclave.Core` | Domain models and errors, shared across layers |
| `EnsyInc.Enclave.DataAccess` | Repository abstractions and entity mappers |
| `EnsyInc.Enclave.DataAccess.EF` | EF Core `DbContext`, entity configuration, repository implementations |
| `EnsyInc.Enclave.Migrations` | Standalone runner that applies EF Core migrations against the database |
| `EnsyInc.Enclave.Bootstrap` | Shared app wiring (configuration, logging, DI) consumed by the Api |
| `EnsyInc.Enclave.UnitTests` | Unit tests for logic not reachable via black-box HTTP tests |
| `EnsyInc.Enclave.ServiceTests` | Black-box HTTP tests against a running Api instance and a real database |

## Running locally

Requires Docker.

```sh
docker compose up --build
```

This starts SQL Server, applies migrations, and starts the Api on `https://localhost:7150` (Swagger UI at `/swagger`).

## Running tests

```sh
dotnet test src/EnsyInc.Enclave/EnsyInc.Enclave.UnitTests/EnsyInc.Enclave.UnitTests.csproj
dotnet test src/EnsyInc.Enclave/EnsyInc.Enclave.ServiceTests/EnsyInc.Enclave.ServiceTests.csproj
```

The service tests need a live SQL Server instance, migrations applied, and the Api running (see `.github/workflows/ci.yml` for the exact sequence used in CI).

## Tech stack

.NET 10 / ASP.NET Core, Entity Framework Core, SQL Server, FluentValidation, NLog, xUnit. CI runs build, unit tests, service tests, and a SonarCloud analysis on every push and pull request to `main`.
