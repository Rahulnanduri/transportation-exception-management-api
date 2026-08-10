# Synthetic data

## Data boundary

Every operational record in this repository is fabricated for an independent portfolio project. No employer dataset, source code, workflow document, internal URL, site code, customer record, carrier record, employee identity, or confidential metric was used.

Names such as `CASE-0001`, `MOV-0001`, `NODE-A`, `HUB-NORTH`, `CARRIER-01`, and `Analyst-A` are intentionally generic inventions. Resemblance to a real operation is coincidental.

## Seed size and method

The application seeds exactly **36 transportation exception cases** after applying EF Core migrations, and only when the cases table is empty.

Generation is deterministic:

- case and movement references use fixed numeric sequences;
- nodes, fictional carrier labels, exception types, severities, statuses, and assignees are selected from fixed arrays;
- timestamps start from the invented `2026-01-15T08:00:00Z` anchor and advance in fixed six-hour increments rather than using the current clock;
- resolved and unresolved lifecycle fields are internally consistent;
- selected cases receive notes built from fixed templates;
- the same source revision creates the same seed records in every empty database;
- startup against a non-empty database does not append or replace records.

There is no random-number generator, network request, downloaded CSV, or external data package in the seed path.

## Coverage purpose

The 36 cases intentionally cover varied combinations needed to exercise the API:

- each generic exception category;
- all four severities;
- active, resolved, and closed statuses;
- assigned and unassigned work;
- resolved and unresolved timestamps;
- routes across fictional node names;
- cases with and without notes.

This variation supports filters, pagination, transition rules, aggregate reports, SLA calculations, and CSV export. It is test/demo coverage—not a statistically representative sample of any transportation network.

## Illustrative SLA thresholds

| Severity | Due-time offset |
| --- | ---: |
| `Critical` | 2 hours |
| `High` | 4 hours |
| `Medium` | 8 hours |
| `Low` | 24 hours |

These thresholds were invented solely to demonstrate due-date calculation and reporting code. They are not copied service levels, contractual commitments, operating standards, escalation procedures, or recommendations for real operations.

The summary and SLA endpoints calculate demonstration statistics over this fabricated dataset. Their outputs must not be described as real performance or business impact.

## User-created records

`POST /api/cases` accepts manually supplied values, but the checked-in examples use only fictional data. Anyone running the repository is responsible for keeping real personal, confidential, and employer information out of local databases, logs, screenshots, issues, and pull requests.

## Licensing and provenance

The application code and deterministic seed definitions were created for this repository and are covered by its MIT License. No third-party or employer dataset is bundled, so the licence does not purport to grant rights over external operational data.
