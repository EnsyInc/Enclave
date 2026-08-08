# EnsyInc.Enclave

Backend for the Licensing service of EnsyInc's Licensing + Monitoring platform. See [README.md](README.md) for the project overview and [docs/00_overview.md](docs/00_overview.md) / [docs/licensing-service.md](docs/licensing-service.md) for domain requirements. Only the `Product` entity (`/products` CRUD) is implemented so far; `Org`, `User`, `License`, `License Request` are designed but not built.

## Solution layout

All projects live under `src/EnsyInc.Enclave/` (solution file: `EnsyInc.Enclave.slnx`):

- `EnsyInc.Enclave.Api` — controllers, request/response models, validators, exception handling
- `EnsyInc.Enclave.Services` — application/domain logic
- `EnsyInc.Enclave.Core` — domain models, errors
- `EnsyInc.Enclave.DataAccess` — repository abstractions, entity↔domain mappers
- `EnsyInc.Enclave.DataAccess.EF` — `DbContext`, EF entity configuration, repository implementations
- `EnsyInc.Enclave.Migrations` — standalone runner that applies EF Core migrations
- `EnsyInc.Enclave.Bootstrap` — shared app wiring (config, logging, DI) via C# extension blocks
- `EnsyInc.Enclave.UnitTests` / `EnsyInc.Enclave.ServiceTests` — see Testing below

## Conventions

- **Result pattern, not exceptions, for expected failures.** Services return `Result<T>` / `Result` (from the `EnsyNet.Core` NuGet package) wrapping domain `Error` records (`src/EnsyInc.Enclave.Core/Errors/`). Controllers `switch` on `{ HasError, Error }` and map known error cases to HTTP responses. The `_` default arm throws `EnsyInc.Enclave.Api.Exceptions.UnhandledResultErrorException` — this signals a missing switch arm (a real bug), not `System.Exception`, and is logged distinctly by `GlobalExceptionHandler`.
- **C# 14 `extension(...)` blocks** are used throughout for mapper/bootstrapping extension methods (e.g. `DataAccess/Mappers/*.cs`, `BootstrappingExtensions.cs`). Any class with 2+ extension blocks trips a Roslyn `CA1708` false positive (the compiler emits a member literally named `extension` per block, which can't be renamed) — suppress it at the class level with `[SuppressMessage("Naming", "CA1708:...", Justification = "...")]`, matching the existing files.
- **`.editorconfig` is strict** — many Roslyn/CA rules are set to `error`, not `warning`. A local `dotnet build` will fail on style violations the same way CI does.
- Request DTOs with non-nullable value-type properties (enums, etc.) should be annotated `[property: JsonRequired]` to avoid silent under-posting defaults.

## Testing

Default to **ServiceTests** (black-box HTTP tests against a real running Api + real SQL Server, see `EnsyInc.Enclave.ServiceTests`). Only add a `UnitTest` for logic that a black-box HTTP test genuinely can't reach (e.g. a branch not observable from the API surface).

```sh
dotnet test src/EnsyInc.Enclave/EnsyInc.Enclave.UnitTests/EnsyInc.Enclave.UnitTests.csproj
dotnet test src/EnsyInc.Enclave/EnsyInc.Enclave.ServiceTests/EnsyInc.Enclave.ServiceTests.csproj
```

ServiceTests need migrations applied and the Api running first — see the `service-tests` job in `.github/workflows/ci.yml` for the exact sequence (SQL Server via Docker service container, `dotnet run` the Migrations project, then the Api, then `curl`-poll `/swagger/v1/swagger.json` before running tests).

## Build

```sh
dotnet build src/EnsyInc.Enclave/EnsyInc.Enclave.slnx -c Release
```

## Docker

`Dockerfile` is multi-stage with two final targets: `api` and `migrations` (build context must be `src/EnsyInc.Enclave`, see `docker-compose.yml`). Both stages run as non-root via `USER $APP_UID`. When choosing a base image tag, try the plain/`latest`-style tag first rather than assuming a `-preview` tag is needed just because the local SDK is a preview build.

## CI / SonarCloud

CI (`.github/workflows/ci.yml`) runs on push/PR to `main`: build → unit-tests + service-tests (coverage collected via `dotnet-coverage`) → SonarCloud analysis (org `ensyinc`, project key `EnsyInc_Enclave`).

- Coverage: unit-test coverage comes from wrapping `dotnet test` directly (in-process). Service-test coverage comes from wrapping the **Api process itself** with `dotnet-coverage collect` (not the test runner) — the tests hit the Api over real HTTP in a separate process, so instrumenting `dotnet test` would capture nothing. The Api is stopped with `SIGINT` after tests finish so `dotnet-coverage` flushes the report before upload. The `service-tests` job's "Apply database migrations" step is likewise wrapped in `dotnet-coverage collect` — it's real code (`EnsyInc.Enclave.Migrations/Program.cs`) that already executes every CI run, so instrumenting it turns it from uncovered (dragging the overall percentage down) into covered, instead of needing a `sonar.coverage.exclusions` carve-out. All three reports are merged by Sonar via a comma-separated `sonar.cs.vscoveragexml.reportsPaths`.
- `sonar.exclusions` excludes `**/*.slnx`, `**/Migrations/*.cs` (EF-generated migration/designer/snapshot files), and `.github/**`.
- The SonarCloud "main branch" designation must stay pointed at `main`, not `master` — if it ever drifts (e.g. after project recreation), fix it via Administration → Branches in the SonarCloud UI (rename or promote), not by changing scanner args.
