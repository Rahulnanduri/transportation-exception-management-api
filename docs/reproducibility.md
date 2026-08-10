# Reproducibility

This project is designed to build and run from source without an external service or dataset. It uses a repository-selected .NET SDK, explicit direct dependency versions, an EF Core migration, and deterministic synthetic seed data.

## Selected toolchain

- .NET SDK: `10.0.302`
- Target framework: `net10.0`
- SDK roll-forward: `latestPatch` within the `10.0.3xx` feature band
- Prerelease SDKs: disabled by `global.json`
- Local EF tool: `dotnet-ef` `10.0.10`

The repository-level `global.json` is authoritative. Confirm the selected SDK before building:

```bash
dotnet --version
```

The expected output for the documented baseline is `10.0.302`. A later installed servicing patch in the same feature band may be selected by the declared roll-forward policy.

## Direct package versions

| Component | Version |
| --- | --- |
| Entity Framework Core SQLite and Design | `10.0.10` |
| SQLitePCLRaw bundle | `3.0.5` |
| Swashbuckle.AspNetCore | `10.2.3` |
| Microsoft.AspNetCore.Mvc.Testing | `10.0.10` |
| Microsoft.NET.Test.Sdk | `17.14.1` |
| xUnit | `2.9.3` |
| xUnit Visual Studio runner | `3.1.4` |
| coverlet collector | `6.0.4` |

These are direct references, not a claim that every transitive dependency is permanently pinned. Restore resolves the transitive graph from configured NuGet sources.

## Clean setup

From the repository root:

```bash
dotnet tool restore
dotnet restore TransportationExceptionManagement.sln
dotnet build TransportationExceptionManagement.sln --configuration Release --no-restore
dotnet test TransportationExceptionManagement.sln --configuration Release --no-build
```

Run the API:

```bash
dotnet run --project src/TransportationExceptionManagement.Api
```

The default launch profile uses `http://localhost:5234`. In the Development environment, inspect Swagger UI at `http://localhost:5234/swagger` and the OpenAPI document at `http://localhost:5234/swagger/v1/swagger.json`.

## Database creation and migration

The infrastructure project contains the committed `InitialCreate` migration. Application startup uses `Database.MigrateAsync`, not `EnsureCreated`, so a missing SQLite file is created by applying the migration.

To apply migrations explicitly:

```bash
dotnet ef database update \
  --project src/TransportationExceptionManagement.Infrastructure \
  --startup-project src/TransportationExceptionManagement.Infrastructure \
  --connection "Data Source=transportation-exceptions.db"
```

Use an absolute path in the connection string when the database location matters; relative design-time paths are resolved from the startup project.

To demonstrate clean creation, stop the API, remove only your generated local database files, then start it again. Never remove a database whose contents need to be retained. SQLite `.db`, `.db-shm`, and `.db-wal` files are ignored by Git.

Override the local connection string without editing committed settings:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Data Source=C:\path\to\temporary\transportation-exceptions.db'
dotnet run --project src\TransportationExceptionManagement.Api
```

Use an appropriate absolute path for the current environment.

## Deterministic seed process

After migrations finish, startup checks whether the cases table is empty. Only an empty database receives the 36 fabricated cases. References, fictional nodes, carriers, assignees, timestamps, status distribution, and notes come from fixed source values; the seed does not download data or depend on the current clock. Repeating a run against the same non-empty database does not duplicate cases.

See [synthetic-data.md](synthetic-data.md) for the data boundary and methodology.

## Quality verification

Run the same checks used by continuous integration:

```bash
dotnet restore TransportationExceptionManagement.sln
dotnet format TransportationExceptionManagement.sln --verify-no-changes --no-restore
dotnet build TransportationExceptionManagement.sln --configuration Release --no-restore
dotnet test TransportationExceptionManagement.sln --configuration Release --no-build
dotnet list TransportationExceptionManagement.sln package --vulnerable --include-transitive
```

The GitHub Actions workflow performs restore, formatting verification, a Release build, tests, and a bounded smoke test against a real running process. The smoke test uses a temporary SQLite file and requires successful responses from `/health` and `/swagger/v1/swagger.json`.

Passing results must come from an actual local command or CI run; this document does not treat historical output as newly reproduced evidence.

## OpenAPI snapshot

`docs/openapi.json` is not hand-authored. It must be captured from the running application after the implementation is complete:

```bash
curl --fail --silent --show-error \
  http://localhost:5234/swagger/v1/swagger.json \
  --output docs/openapi.json
```

Regenerate the snapshot whenever the public API contract changes, then review the diff.

## Environment assumptions

- Windows, macOS, and Linux are expected to work with a supported .NET 10 SDK and a compatible native SQLite runtime.
- The first restore requires access to configured NuGet package sources.
- Local HTTP examples assume port `5234`; CI supplies a separate loopback URL.
- Swagger endpoints are enabled in `Development`.
- No Docker daemon, database server, external API, credential, or private package feed is required.
- Time-based report values are evaluated at request time against fixed seed timestamps, so overdue totals can change as the current date advances even though the underlying seed records remain deterministic.
