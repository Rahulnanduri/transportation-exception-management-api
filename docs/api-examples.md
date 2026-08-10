# API examples

These examples assume the API is running in Development at `http://localhost:5234`. Every payload uses invented portfolio data. Commands use `curl`; Windows PowerShell users can invoke `curl.exe` to avoid the legacy `curl` alias in older PowerShell versions.

Set a convenience variable in a Bash-compatible shell:

```bash
base_url="http://localhost:5234"
```

## Health and API discovery

```bash
curl --fail --silent --show-error "$base_url/health"
curl --fail --silent --show-error "$base_url/swagger/v1/swagger.json"
```

Swagger UI is available at `$base_url/swagger` in Development.

## List cases

```bash
curl --get "$base_url/api/cases" \
  --data-urlencode "page=1" \
  --data-urlencode "pageSize=10" \
  --data-urlencode "sortBy=CreatedAtUtc" \
  --data-urlencode "sortDirection=Desc"
```

The response envelope has this shape:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

The values above demonstrate the contract shape; a freshly seeded local database returns populated `items` and corresponding totals.

## Filter and sort cases

Filters can be combined. Enum query values use their readable names.

```bash
curl --get "$base_url/api/cases" \
  --data-urlencode "status=InProgress" \
  --data-urlencode "severity=High" \
  --data-urlencode "exceptionType=DeliveryRisk" \
  --data-urlencode "assignee=Analyst-A" \
  --data-urlencode "origin=NODE-A" \
  --data-urlencode "createdFrom=2026-01-01T00:00:00Z" \
  --data-urlencode "page=1" \
  --data-urlencode "pageSize=20" \
  --data-urlencode "sortBy=DueAtUtc" \
  --data-urlencode "sortDirection=Asc"
```

Supported case-list filters are `status`, `severity`, `exceptionType`, `assignee`, `origin`, `destination`, `createdFrom`, and `createdTo`. Supported sort fields are `CreatedAtUtc`, `UpdatedAtUtc`, `DueAtUtc`, `Severity`, `Status`, and `CaseReference`.

## Get one case

```bash
curl --fail --silent --show-error "$base_url/api/cases/1"
```

The detail representation includes the description, lifecycle timestamps, resolution fields, and notes. An unknown integer ID returns `404 Not Found`.

## Create a case

Use a unique case reference when repeating this request:

```bash
curl --include \
  --request POST "$base_url/api/cases" \
  --header "Content-Type: application/json" \
  --data '{
    "caseReference": "CASE-DEMO-1001",
    "movementReference": "MOV-DEMO-1001",
    "originNode": "NODE-A",
    "destinationNode": "DC-EAST",
    "carrierCode": "CARRIER-04",
    "exceptionType": "RouteDisruption",
    "severity": "High",
    "description": "Synthetic route disruption created for API demonstration."
  }'
```

A valid request returns `201 Created`, includes a `Location` header for the case-detail route, starts in `New`, and calculates `dueAtUtc` from the illustrative severity threshold. Reusing a case reference returns a conflict. Identical origin and destination values fail validation.

Save the returned integer `id` for the following commands:

```bash
case_id="37"
```

`37` is only an example for the first manual record after 36 seeded cases; use the actual response ID in your database.

## Assign a case

```bash
curl --fail --silent --show-error \
  --request PATCH "$base_url/api/cases/$case_id/assignment" \
  --header "Content-Type: application/json" \
  --data '{"assignee":"Analyst-D"}'
```

Assignment is required before a case can enter `InProgress`.

## Change status

Move the assigned `New` case to active work:

```bash
curl --fail --silent --show-error \
  --request PATCH "$base_url/api/cases/$case_id/status" \
  --header "Content-Type: application/json" \
  --data '{"status":"InProgress"}'
```

## Add a note

```bash
curl --fail --silent --show-error \
  --request POST "$base_url/api/cases/$case_id/notes" \
  --header "Content-Type: application/json" \
  --data '{
    "author": "Analyst-D",
    "text": "Synthetic follow-up note for the portfolio example."
  }'
```

## Resolve and close the case

Resolve it with an explicit synthetic summary:

```bash
curl --fail --silent --show-error \
  --request PATCH "$base_url/api/cases/$case_id/status" \
  --header "Content-Type: application/json" \
  --data '{
    "status": "Resolved",
    "resolutionSummary": "Synthetic rerouting example completed."
  }'
```

An invalid transition—or a transition to `Resolved` without a non-empty summary—returns `409 Conflict` with `ProblemDetails`.

Close the resolved case:

```bash
curl --fail --silent --show-error \
  --request PATCH "$base_url/api/cases/$case_id/status" \
  --header "Content-Type: application/json" \
  --data '{"status":"Closed"}'
```

## Summary report

```bash
curl --fail --silent --show-error "$base_url/api/reports/summary"
```

The report aggregates total, active, resolved, and overdue-active cases plus counts by status, severity, exception type, and assignee.

## SLA report

```bash
curl --fail --silent --show-error "$base_url/api/reports/sla"
```

The output describes performance against invented demonstration thresholds only. It is not a real-company SLA report.

## CSV export

Export all cases:

```bash
curl --fail --silent --show-error \
  "$base_url/api/cases/export.csv" \
  --output transportation-cases.csv
```

Export a filtered subset:

```bash
curl --get --fail --silent --show-error \
  "$base_url/api/cases/export.csv" \
  --data-urlencode "status=InProgress" \
  --data-urlencode "severity=High" \
  --output active-high-severity-cases.csv
```

The response uses `text/csv`, deterministic ordering, and CSV escaping for embedded commas, quotes, and line breaks.

## Validation and errors

Malformed DTOs and unsupported enum values return validation errors. Missing cases return `404`. Duplicate case references and invalid lifecycle transitions return conflicts. Error bodies follow ASP.NET Core `ProblemDetails`; clients should use the HTTP status and structured fields rather than parse human-readable text.
