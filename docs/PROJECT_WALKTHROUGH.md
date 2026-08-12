# Transportation Exception Management API

## Executive Summary

Transportation Exception Management API is an independent C#/.NET portfolio project that models how transportation exceptions can be recorded, assigned, progressed, explained, reported, and exported through a consistent backend workflow. It turns a familiar operations problem - delayed, constrained, disrupted, or documentation-blocked movements - into an auditable API rather than a collection of disconnected CRUD endpoints.

The implementation uses ASP.NET Core controllers, application services, a domain state machine, Entity Framework Core, and SQLite. It includes deterministic synthetic seed data, illustrative severity-based due times, filtered and paginated case search, case notes, summary and SLA-oriented reporting, CSV export, standard HTTP error semantics, migrations, and 36 integration tests. A live Swagger walkthrough on Windows exercised all ten public operations in a coherent business flow and verified persistence in both later API reads and the SQLite database.

This is original portfolio engineering evidence. It is not an Amazon application, an employer system, a commercial platform, or a representation of professional production C#/.NET experience. All cases, movements, nodes, carriers, analysts, timestamps, reports, and workflow thresholds are fabricated for deterministic demonstration. No real customer, carrier, employee, or operational data is included.

## Why This Project Exists

Transportation exception handling combines operational judgment with system design. Teams need to know what went wrong, which movement is affected, who owns the response, which state the case is in, whether a requested transition is valid, and whether the exception is approaching or exceeding an agreed due time. A useful system must preserve that context while making aggregate workload visible.

This project was built to demonstrate that problem in code. Its portfolio value is not a claimed business deployment; it is the ability to translate a transportation-domain workflow into explicit entities, invariants, REST contracts, persistence rules, reports, and tests. The scope is deliberately self-contained so another engineer can clone it, create the database, run it, and reproduce the same baseline without credentials or an external data source.

## Transportation Domain Model

The central entity is a `TransportationExceptionCase`. It connects a case reference to a fictional movement, an origin and destination node, a fictional carrier code, an exception category, severity, status, description, assignee, lifecycle timestamps, and zero or more notes.

- **Movement context:** `MovementReference`, `OriginNode`, and `DestinationNode` identify the fabricated movement and route endpoints. Origin and destination must differ.
- **Exception classification:** supported types are `PickupDelay`, `DeliveryRisk`, `CapacityConstraint`, `DocumentationIssue`, `EquipmentIssue`, and `RouteDisruption`.
- **Severity:** `Low`, `Medium`, `High`, and `Critical` drive an illustrative due-time calculation.
- **Ownership:** an assignee records who owns the next action. Assignment is required before a new case can enter active work.
- **Lifecycle:** cases move through `New`, `InProgress`, `WaitingExternal`, `Resolved`, and `Closed` along explicitly permitted edges.
- **Narrative history:** notes append contextual updates with an author label and UTC timestamp.
- **Reporting:** snapshots support workload counts, active and overdue totals, due-time compliance, average resolution time, and groupings by status, severity, exception type, and assignee.

The labels are intentionally generic. `CARRIER-01` and `Analyst-A`, for example, are invented identifiers rather than real organizations or people.

## System Architecture

The project names correspond to real dependency boundaries confirmed in the project references and implementation:

```mermaid
flowchart LR
    Client["Client / Swagger"] --> Api["ASP.NET Core API"]
    Api --> App["Application services and DTOs"]
    App --> Domain["Domain entities and rules"]
    App --> Contract["ICaseRepository"]
    Infra["Infrastructure / EF Core"] -. implements .-> Contract
    Infra --> Domain
    Infra --> DbContext["TransportationExceptionsDbContext"]
    DbContext --> SQLite[("SQLite")]
    Tests["xUnit integration tests"] --> Api
    Tests --> SQLite
```

The domain is the dependency root and has no ASP.NET Core or EF Core reference. The Application project depends on Domain and defines use cases plus the repository contract. Infrastructure depends on Application and Domain to implement the contract with EF Core. API references Application and Infrastructure and acts as the composition root. Tests host the actual API and replace only database and time dependencies.

### Request flow

1. ASP.NET Core binds and validates a request DTO.
2. A controller delegates the use case to `CaseService`, `ReportService`, or `CaseCsvExportService`.
3. Application logic invokes domain methods and repository operations.
4. The EF Core repository translates filters, ordering, projections, and updates to SQLite.
5. Entities are mapped to response DTOs; controllers return the intended HTTP status.
6. Validation, missing resources, duplicates, and workflow conflicts are represented with structured 4xx responses.

## Repository Structure

| Project or path | Confirmed responsibility |
| --- | --- |
| `src/TransportationExceptionManagement.Domain` | Case and note entities, enums, field limits, workflow conflicts, transition rules, and illustrative SLA policy. |
| `src/TransportationExceptionManagement.Application` | DTOs, filters, pagination, mappings, service interfaces and implementations, repository contract, reports, and CSV export. |
| `src/TransportationExceptionManagement.Infrastructure` | SQLite registration, `DbContext`, mappings, repository implementation, migration, initializer, and deterministic seeder. |
| `src/TransportationExceptionManagement.Api` | Controllers, dependency composition, JSON enums, Swagger, health, validation response customization, and global exception handling. |
| `tests/TransportationExceptionManagement.Tests` | xUnit integration tests using `WebApplicationFactory`, SQLite in-memory persistence, and a fixed clock. |
| `.github/workflows/ci.yml` | Linux restore, format verification, Release build, tests, and a running-process smoke test. |
| `docs/` and `requests.http` | Architecture, data model, provenance, reproducibility, request examples, OpenAPI snapshot, and this validated walkthrough. |

`Directory.Build.props` enables nullable reference types, implicit usings, deterministic compilation, and warnings as errors across the solution. `global.json` selects .NET SDK `10.0.302`, while `dotnet-tools.json` pins `dotnet-ef` `10.0.10`.

## Technology Stack

| Component | Verified version or implementation |
| --- | --- |
| Language and runtime | C# on `net10.0`; .NET SDK `10.0.302` |
| Web framework | ASP.NET Core controllers and `ProblemDetails` |
| Persistence | Entity Framework Core SQLite `10.0.10` |
| SQLite native bundle | `SQLitePCLRaw.bundle_e_sqlite3` `3.0.5` |
| API discovery | Swashbuckle.AspNetCore `10.2.3`, OpenAPI 3.0, Swagger UI |
| Testing | xUnit `2.9.3`, `Microsoft.AspNetCore.Mvc.Testing` `10.0.10` |
| Coverage integration | coverlet collector `6.0.4` |
| CI | GitHub Actions on `ubuntu-latest` |

## Data & Privacy

The seed path generates exactly 36 cases when the database is empty. It starts from the invented timestamp `2026-01-15T08:00:00Z`, advances in fixed six-hour intervals, and cycles through fixed arrays of generic nodes, carrier labels, assignees, exception types, severities, and statuses. Selected cases receive deterministic notes. There is no random generator, download, current-time dependency, employer source, or third-party data package in the seed process.

This boundary is intentional:

- deterministic synthetic transportation data only;
- illustrative workflow and due-time rules only;
- no Amazon internal application, process, terminology, code, or data;
- no real carrier, customer, shipment, employee, or site information;
- no claimed production deployment, user base, scale, or business impact.

## API Surface

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/health` | Confirm that the process is responsive. |
| `GET` | `/api/cases` | Filter, sort, and paginate case summaries. |
| `POST` | `/api/cases` | Create a validated synthetic case and calculate its due time. |
| `GET` | `/api/cases/export.csv` | Export a filtered, deterministically ordered case set. |
| `GET` | `/api/cases/{id}` | Return case detail, lifecycle fields, and notes. |
| `PATCH` | `/api/cases/{id}/assignment` | Assign case ownership. |
| `PATCH` | `/api/cases/{id}/status` | Apply a validated lifecycle transition. |
| `POST` | `/api/cases/{id}/notes` | Append a validated contextual note. |
| `GET` | `/api/reports/summary` | Return workload counts and aggregate dimensions. |
| `GET` | `/api/reports/sla` | Return illustrative due-time outcomes and the policy disclaimer. |

The case list accepts `status`, `severity`, `exceptionType`, `assignee`, `origin`, `destination`, `createdFrom`, and `createdTo`, plus pagination and a whitelist of sort fields. Enum values are serialized as readable strings and unknown enum text is rejected.

## End-to-End Workflow

The live walkthrough used case ID `37`, reference `CASE-VALIDATION-20260811-01`, and only fabricated content.

1. Confirmed `/health` returned `200` and `{"status":"Healthy"}`.
2. Listed three deterministic cases with ascending case-reference ordering; the response reported 36 seeded records before mutation.
3. Created a High-severity `RouteDisruption` from `HUB-NORTH` to `DC-WEST`; creation returned `201`, `Location: /api/cases/37`, `New`, and a due time four hours after creation.
4. Retrieved case 37 with `200` and verified its initial unassigned state.
5. Attempted `New -> InProgress` before assignment. The API returned `409` with code `assignee_required` when `Accept: application/json` was selected.
6. Assigned the case to `Analyst-Portfolio`; the API returned `200` and updated the lifecycle timestamp.
7. Added note 13; the API returned `201` and a detail URL in the `Location` header.
8. Transitioned the assigned case from `New` to `InProgress`; the API returned `200`.
9. Retrieved case 37 again and confirmed the assignee, status, and note persisted.
10. Attempted the invalid edge `InProgress -> Closed`. The API returned `409`, code `invalid_transition`, and left the case in `InProgress`.
11. Ran summary and SLA reports after the mutation.
12. Exported the `Analyst-Portfolio` subset as CSV and verified a 12-column header plus one matching data row.

## Runtime Validation

Validation was performed locally on Windows on 11 August 2026.

| Check | Result |
| --- | --- |
| Selected SDK | .NET SDK `10.0.302` |
| Solution | `TransportationExceptionManagement.sln` |
| API startup | Successful at `http://localhost:5234` |
| Database initialization | Initial EF Core migration applied; deterministic seed present |
| Swagger | Loaded successfully from the running API |
| Automated tests | 36 passed, 0 failed, 0 skipped |
| Runtime workflow | All ten public operations exercised through the existing Swagger session |
| Persistence | Later detail GET and read-only SQLite inspection matched the mutations |
| Remote CI | Latest checked run, CI #2 on commit `8710758`, completed successfully on 10 August 2026 ([run details](https://github.com/Rahulnanduri/transportation-exception-management-api/actions/runs/31391980858)) |

### Endpoint results

| Method and route | Request or filter | Status | Key observed behaviour | Persistence or rule check |
| --- | --- | ---: | --- | --- |
| `GET /health` | none | 200 | `status: Healthy` | Confirms running process. |
| `GET /api/cases` | page 1, size 3, case reference ascending | 200 | `CASE-0001` through `CASE-0003`; total 36 | Deterministic ordering and pagination metadata matched. |
| `POST /api/cases` | synthetic High `RouteDisruption` | 201 | Created ID 37 in `New`; four-hour due time; detail `Location` | Database count increased from 36 to 37. |
| `GET /api/cases/37` | ID 37 | 200 | Returned complete lifecycle and notes | Final read contained assignment, `InProgress`, and note 13. |
| `PATCH /api/cases/37/assignment` | `Analyst-Portfolio` | 200 | Assignee set and `updatedAtUtc` advanced | Later GET and SQLite row matched. |
| `PATCH /api/cases/37/status` | `InProgress` | 409, then 200 | Rejected while unassigned; succeeded after assignment | Confirmed `assignee_required`, then valid edge. |
| `POST /api/cases/37/notes` | synthetic analyst note | 201 | Created note 13 | Later GET and foreign-key row matched. |
| `PATCH /api/cases/37/status` | invalid `Closed` from `InProgress` | 409 | `invalid_transition`; state remained `InProgress` | Invalid command produced no state change. |
| `GET /api/reports/summary` | none | 200 | Total 37, active 23, resolved 14, closed 7, overdue active 22 | Dimension totals reconciled to 37. |
| `GET /api/reports/sla` | none | 200 | 7 within, 7 after, 50% illustrative compliance, 10.8 average hours | Included all four synthetic thresholds and disclaimer. |
| `GET /api/cases/export.csv` | assignee `Analyst-Portfolio` | 200 | Attachment, `text/csv`, two lines, 12 columns | Export row matched case 37 and its current state. |

Representative bodies are intentionally abbreviated here; the screenshots show the actual live responses.

## Swagger Walkthrough

### Contract overview

![Swagger overview showing the ten public operations](images/01-swagger-overview.png)

### Health and deterministic listing

![Health endpoint returning HTTP 200 and Healthy](images/02-health-200.png)

![Paginated case list returning HTTP 200](images/03-cases-list-200.png)

### Created case and persisted workflow

![Synthetic case creation returning HTTP 201](images/04-case-created.png)

![Case detail showing the persisted assignee status and note](images/05-case-detail.png)

![Assignment mutation returning HTTP 200](images/06-case-assignment.png)

![Valid status transition returning HTTP 200](images/07-case-status-transition.png)

![Case note creation returning HTTP 201](images/08-case-note.png)

### Reporting, export, and conflict handling

![Summary report returning reconciled counts](images/09-summary-report.png)

![SLA report returning illustrative thresholds and disclaimer](images/10-sla-report.png)

![Filtered CSV export returning an attachment](images/11-csv-export.png)

![Invalid InProgress to Closed transition returning HTTP 409](images/12-invalid-transition-409.png)

## Domain / Workflow Rules

### 1. Controlled state transitions

**Business purpose:** prevent cases from skipping operationally meaningful stages or moving along ambiguous edges.

**Implementation:** `TransportationExceptionCase.IsTransitionAllowed` defines the complete state graph. Any other edge throws `CaseWorkflowException` with `invalid_transition`.

**API behaviour:** the exception handler maps workflow conflicts to `409 Conflict` and includes a stable code when JSON problem details are negotiated.

**Test coverage:** integration tests cover assigned progress, direct resolution rejection, resolve-and-close, reopening, and the unassigned guard. The live walkthrough also rejected `InProgress -> Closed` and verified no state change.

```mermaid
stateDiagram-v2
    [*] --> New
    New --> InProgress: assignee required
    InProgress --> WaitingExternal
    InProgress --> Resolved: summary required
    WaitingExternal --> InProgress
    WaitingExternal --> Resolved: summary required
    Resolved --> Closed
    Resolved --> InProgress: reopen
    Closed --> InProgress: reopen
```

### 2. Ownership before active work

**Business purpose:** active work should have explicit ownership.

**Implementation:** `ChangeStatus` blocks `InProgress` when `Assignee` is blank.

**API behaviour:** an unassigned `New` case returns `409` with `assignee_required`. After assignment, the same command returns `200`.

**Test coverage:** `Status_UnassignedNewCaseCannotEnterInProgress`, `Status_AssignedNewCaseCanEnterInProgress`, and assignment validation tests protect both paths.

### 3. Resolution completeness and reopening

**Business purpose:** resolution should explain what closed the exception, while reopening should not preserve a stale current resolution.

**Implementation:** a non-empty, length-limited summary is required for `Resolved`. Moving from `Resolved` or `Closed` back to `InProgress` clears `ResolvedAtUtc` and `ResolutionSummary`.

**API behaviour:** missing summaries and unsupported transitions return `409`; successful resolution sets both lifecycle fields.

**Test coverage:** resolve-without-summary, resolve-then-close, and reopen-clears-resolution tests exercise these rules.

### 4. Monotonic lifecycle timestamps

**Business purpose:** assignment, notes, and state changes should not make the audit timeline move backwards.

**Implementation:** the entity normalizes timestamps to UTC and rejects occurrences earlier than the current `UpdatedAtUtc`.

**API behaviour:** successful mutations advance `updatedAtUtc`; note and status responses expose their UTC timestamps.

**Test coverage:** the fixed-time integration environment makes creation, due-time, assignment, and resolution assertions deterministic.

### 5. Illustrative severity-based due times

**Business purpose:** demonstrate prioritization and due-time reporting without claiming a real SLA.

**Implementation:** Critical = 2 hours, High = 4, Medium = 8, and Low = 24. The due timestamp is created time plus the configured threshold.

**API behaviour:** case creation returns the calculated due time; the SLA endpoint returns threshold metadata and an explicit synthetic-data disclaimer.

**Test coverage:** creation checks a Critical case receives a two-hour due time; SLA tests reconcile the seeded outcomes and disclaimer.

### 6. Input and identity constraints

Case references are unique under SQLite `NOCASE` collation. Required strings are trimmed and length-limited. Origin and destination must be different after trimming and case-insensitive comparison. Unknown enum strings fail request binding. Notes require a non-empty author and text. Duplicate references produce `409`; shape and annotation failures produce `400` validation problems.

## Database & Persistence

The API uses `Data Source=transportation-exceptions.db` by default. The path is relative to the API's runtime working directory; generated database and SQLite sidecar files are ignored by Git and are not part of the public documentation.

`TransportationExceptionsDbContext` exposes case and note sets and applies all entity configurations from the Infrastructure assembly. The initial migration creates:

- `TransportationExceptionCases`, with a case identity and a unique case-reference index;
- `CaseNotes`, with a required foreign key to the parent case;
- cascade deletion for child notes;
- indexes on status, severity, exception type, assignee, created time, due time, and note ordering;
- string storage for enums and Unix-millisecond integer storage for UTC timestamps.

Startup calls `Database.MigrateAsync` and then invokes the deterministic seeder. The seeder returns immediately when any case already exists, preventing repeated startup from duplicating the 36-record baseline.

Read-only inspection after the Swagger flow verified migration `20260810125518_InitialCreate`, EF product version `10.0.10`, the declared cascading note foreign key, 37 cases, 13 notes, case 37 in `InProgress`, and note 13 linked to that case. This database evidence matched the final API detail response.

## Reporting

The summary service projects lightweight snapshots rather than loading complete tracked entities. It treats `New`, `InProgress`, and `WaitingExternal` as active, counts an active case as overdue when `DueAtUtc < now`, and groups by status, severity, exception type, and normalized assignee label.

The SLA report uses cases with a `ResolvedAtUtc` value. Resolution is within the illustrative target when `ResolvedAtUtc <= DueAtUtc`; percentage and average resolution hours are rounded to two decimal places. At validation time, the deterministic seed produced seven within and seven after, or 50%, and the active runtime case did not change those resolved metrics.

CSV export applies the same filter model as case search, orders by case reference and ID, uses invariant round-trip timestamps, quotes embedded CSV content, and prefixes formula-like values beginning with `=`, `+`, `-`, or `@`. The last step reduces spreadsheet formula-injection risk when exported data is opened by a spreadsheet application.

## Testing Strategy

The test project contains 36 xUnit integration tests. `TestApiFactory` derives from `WebApplicationFactory<Program>`, hosts the real ASP.NET Core pipeline, replaces file-based persistence with one open SQLite in-memory connection, and replaces system time with a fixed `TimeProvider`. SQLite is used instead of EF Core's InMemory provider, so migrations, indexes, uniqueness, value conversion, LINQ translation, and relational persistence remain in the test boundary.

Representative scenarios include:

1. **Creation and due time:** verifies `201`, the `Location` resource, initial `New` status, fixed creation time, and Critical +2-hour due time.
2. **Assignment guard:** verifies an unassigned case cannot enter active work and exposes `assignee_required`.
3. **Resolve / close / reopen:** verifies required summaries, resolved lifecycle fields, permitted close, and clearing stale resolution data when reopened.
4. **Notes persistence:** creates a note through HTTP and then retrieves the case to prove it was stored.
5. **Reporting reconciliation:** confirms baseline totals and that grouped counts sum to all 36 seeded cases.
6. **Migration-backed startup:** confirms the initial migration is applied, exactly 36 cases are seeded, and a second seeder call does not duplicate them.

These tests improve confidence because they protect observable behaviour across HTTP, application orchestration, domain rules, EF Core, and SQLite rather than testing each class in isolation with mocks.

### Selected test excerpts

`tests/TransportationExceptionManagement.Tests/Api/CaseWorkflowTests.cs` - due-time policy and created-resource semantics:

```csharp
var (response, created) = await CreateAsync(
    TestCaseRequests.Create("TEST-CREATE-001", severity: ExceptionSeverity.Critical));

Assert.Equal(HttpStatusCode.Created, response.StatusCode);
Assert.EndsWith($"/api/cases/{created.Id}",
    response.Headers.Location?.OriginalString, StringComparison.Ordinal);
Assert.Equal(CaseStatus.New, created.Status);
Assert.Equal(TestApiFactory.FixedUtcNow.AddHours(2), created.DueAtUtc);
```

`tests/TransportationExceptionManagement.Tests/Api/CaseWorkflowTests.cs` - ownership guard:

```csharp
var (_, created) = await CreateAsync(TestCaseRequests.Create("TEST-UNASSIGNED"));

var response = await ChangeStatusAsync(created.Id, CaseStatus.InProgress);

Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
Assert.Equal("assignee_required", await response.ReadProblemCodeAsync());
```

`tests/TransportationExceptionManagement.Tests/Api/CaseWorkflowTests.cs` - persistence of notes through a later read:

```csharp
var note = await response.ReadJsonAsync<CaseNoteDto>();
var detail = await _client.GetFromJsonAsync<CaseDetailDto>(
    $"/api/cases/{created.Id}", TestHttp.JsonOptions);

Assert.Equal(HttpStatusCode.Created, response.StatusCode);
Assert.True(note.Id > 0);
Assert.Contains(detail!.Notes,
    item => item.Id == note.Id && item.Text == note.Text);
```

`tests/TransportationExceptionManagement.Tests/Api/ReadEndpointsTests.cs` - report reconciliation:

```csharp
Assert.NotNull(summary);
Assert.Equal(36, summary.TotalCases);
Assert.Equal(22, summary.ActiveCases);
Assert.Equal(14, summary.ResolvedCases);
Assert.Equal(7, summary.ClosedCases);
Assert.Equal(36, summary.CountsByStatus.Values.Sum());
Assert.Equal(36, summary.CountsBySeverity.Values.Sum());
```

`tests/TransportationExceptionManagement.Tests/Infrastructure/DatabaseInitializationTests.cs` - migrations and deterministic seed size:

```csharp
var migrations = await database.Database.GetAppliedMigrationsAsync();
var caseCount = await database.Cases.CountAsync();

Assert.Contains(migrations,
    migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
Assert.Equal(SyntheticDataSeeder.SeedCaseCount, caseCount);
Assert.Equal(36, caseCount);
```

## Selected Implementation Highlights

### Domain state machine

`src/TransportationExceptionManagement.Domain/Entities/TransportationExceptionCase.cs`

```csharp
private static bool IsTransitionAllowed(CaseStatus currentStatus, CaseStatus requestedStatus) =>
    (currentStatus, requestedStatus) switch
    {
        (CaseStatus.New, CaseStatus.InProgress) => true,
        (CaseStatus.InProgress, CaseStatus.WaitingExternal or CaseStatus.Resolved) => true,
        (CaseStatus.WaitingExternal, CaseStatus.InProgress or CaseStatus.Resolved) => true,
        (CaseStatus.Resolved, CaseStatus.Closed or CaseStatus.InProgress) => true,
        (CaseStatus.Closed, CaseStatus.InProgress) => true,
        _ => false,
    };
```

This compact switch is the authoritative lifecycle graph. It keeps transition logic in the entity, independent of controllers and persistence.

### Assignment and resolution guards

`src/TransportationExceptionManagement.Domain/Entities/TransportationExceptionCase.cs`

```csharp
if (requestedStatus == CaseStatus.InProgress && string.IsNullOrWhiteSpace(Assignee))
{
    throw new CaseWorkflowException(
        "assignee_required",
        "A case must have an assignee before moving to InProgress.",
        Status,
        requestedStatus);
}

if (requestedStatus == CaseStatus.Resolved && string.IsNullOrWhiteSpace(resolutionSummary))
{
    throw new CaseWorkflowException(
        "resolution_summary_required",
        "A non-empty resolution summary is required when resolving a case.",
        Status,
        requestedStatus);
}
```

The rule codes give API clients stable machine-readable reasons while the messages remain human-readable.

### Illustrative SLA policy

`src/TransportationExceptionManagement.Domain/Policies/IllustrativeSlaPolicy.cs`

```csharp
new Dictionary<ExceptionSeverity, TimeSpan>
{
    [ExceptionSeverity.Critical] = TimeSpan.FromHours(2),
    [ExceptionSeverity.High] = TimeSpan.FromHours(4),
    [ExceptionSeverity.Medium] = TimeSpan.FromHours(8),
    [ExceptionSeverity.Low] = TimeSpan.FromHours(24),
};
```

The values are centralized, deterministic, and explicitly documented as portfolio-only rules.

### Queryable repository filters

`src/TransportationExceptionManagement.Infrastructure/Persistence/Repositories/CaseRepository.cs`

```csharp
if (filter.Status.HasValue)
{
    query = query.Where(entity => entity.Status == filter.Status.Value);
}

if (!string.IsNullOrWhiteSpace(filter.Origin))
{
    var originPattern = $"%{filter.Origin.Trim()}%";
    query = query.Where(entity => EF.Functions.Like(entity.OriginNode, originPattern));
}
```

Filters remain composable LINQ expressions, translated by EF Core into parameterized SQL. Sorting is separately whitelisted through an enum switch.

### SLA report calculation

`src/TransportationExceptionManagement.Application/Reports/ReportService.cs`

```csharp
var resolved = cases.Where(item => item.ResolvedAtUtc.HasValue).ToArray();
var within = resolved.Count(item => item.ResolvedAtUtc <= item.DueAtUtc);
var after = resolved.Length - within;
decimal? compliance = resolved.Length == 0
    ? null
    : Math.Round(within * 100m / resolved.Length, 2);
decimal? averageHours = resolved.Length == 0
    ? null
    : Math.Round((decimal)resolved.Average(item =>
        (item.ResolvedAtUtc!.Value - item.CreatedAtUtc).TotalHours), 2);
```

The service handles the no-resolved-cases case explicitly and separates resolved performance from currently overdue active work.

### CSV safety and correctness

`src/TransportationExceptionManagement.Application/Exports/CaseCsvExportService.cs`

```csharp
private static string Escape(string value)
{
    if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@')
    {
        value = $"'{value}";
    }

    if (value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n'))
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    return value;
}
```

This covers normal CSV quoting and reduces formula interpretation risk for spreadsheet consumers.

## CI/CD

The `CI` workflow runs for every pull request and push to `main`. It grants read-only repository contents permission, cancels superseded runs on the same ref, uses a 15-minute job timeout, and performs:

1. checkout with `actions/checkout@v5`;
2. SDK selection from `global.json` with `actions/setup-dotnet@v5`;
3. solution restore;
4. `dotnet format --verify-no-changes`;
5. Release build without a second restore;
6. Release tests without a second build;
7. a bounded running-process smoke test against a temporary SQLite file.

The smoke step starts the built API on loopback, polls health for a maximum of 30 attempts, requires non-empty health and OpenAPI responses, and stops the background process through a cleanup trap. The latest remotely checked run was successful; that status is an observed GitHub run, not an inference from the workflow file.

## Engineering Decisions

- **Layered but compact:** separate projects make dependencies inspectable without adding a mediator, mapping framework, or generic repository that would obscure this small service.
- **SQLite for reproducibility:** it provides real relational behaviour without requiring a database server or credentials. It is appropriate for local evaluation, not a scale claim.
- **Domain methods own lifecycle rules:** the API and tests cannot bypass assignment, summary, timestamp, and transition checks through ordinary entity use.
- **DTOs are the public contract:** EF entities are not serialized directly, reducing accidental persistence coupling.
- **String enums and structured errors:** the API is easier to inspect in Swagger and safer for client integration than numeric enums or free-form error text.
- **Deterministic time in tests:** a fixed `TimeProvider` makes due times, resolution times, and reports predictable.
- **Migration on startup:** convenient for a self-contained demonstration. A controlled production environment would normally separate schema promotion.
- **Synthetic policy labels:** names and report disclaimers prevent demonstration thresholds from being confused with real operational standards.

## Limitations

The project intentionally does not attempt to provide:

- authentication, authorization, or authenticated audit identities;
- optimistic concurrency or conflict detection between simultaneous editors;
- external carrier/customer integrations, EDI, telematics, or event streaming;
- notifications, escalation channels, or background workers;
- production observability, distributed tracing backends, metrics infrastructure, or alerting;
- cloud deployment, infrastructure as code, high availability, or horizontal scale;
- a frontend beyond Swagger;
- validated real-world SLA policy, time-zone presentation, or operating impact.

Runtime validation also identified one API-description usability issue: response metadata lists `text/plain` before JSON for many JSON-returning actions, so Swagger selects `Accept: text/plain` by default. Successful JSON responses still render, but exception handling falls back to a generic problem body under that negotiation. Selecting `application/json` exposes the intended `detail`, `instance`, and stable `code`. No application change was made during this documentation task.

## What This Project Demonstrates

- transportation exception and movement-context modelling;
- C#/.NET and ASP.NET Core REST API design;
- non-trivial workflow validation and error semantics;
- application service and repository boundaries;
- Entity Framework Core mappings and migrations;
- SQLite relational persistence;
- deterministic synthetic data and reproducibility;
- integration testing across HTTP, domain, and database layers;
- summary and illustrative SLA reporting;
- safe, filtered CSV export;
- CI and running-process smoke validation;
- evidence-led technical documentation.

## Run Locally

Prerequisites: Git and a compatible stable .NET 10 SDK. No database server, credential, private package feed, or external dataset is required.

```bash
git clone https://github.com/Rahulnanduri/transportation-exception-management-api.git
cd transportation-exception-management-api
dotnet tool restore
dotnet restore TransportationExceptionManagement.sln
dotnet run --project src/TransportationExceptionManagement.Api
```

The Development launch profile uses `http://localhost:5234`. Startup applies pending migrations and seeds only an empty database.

## Test Locally

```bash
dotnet clean TransportationExceptionManagement.sln
dotnet build TransportationExceptionManagement.sln
dotnet test TransportationExceptionManagement.sln --no-build
```

The validated test result is 36 passed, 0 failed, 0 skipped.

## API Documentation

With the API running in Development:

- Swagger UI: `http://localhost:5234/swagger/index.html`
- OpenAPI JSON: `http://localhost:5234/swagger/v1/swagger.json`

The checked-in `docs/openapi.json` is a snapshot from the running application rather than a hand-authored contract.

## Portfolio Context

This project supports a broader career narrative:

**SUPPLY CHAIN -> ANALYTICS -> AUTOMATION -> SYSTEMS**

For a recruiter, it is fast evidence that transportation-domain concepts can be expressed as a functioning, testable system. For an operations or analytics hiring manager, it shows case ownership, lifecycle discipline, due-time reasoning, reporting, and exportability. For a technical interviewer, it provides inspectable domain rules, dependency direction, migrations, relational tests, runtime evidence, and clear limitations without claiming production experience the project does not represent.
