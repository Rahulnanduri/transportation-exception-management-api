using Microsoft.EntityFrameworkCore;
using TransportationExceptionManagement.Domain.Entities;
using TransportationExceptionManagement.Domain.Enums;
using TransportationExceptionManagement.Domain.Policies;

namespace TransportationExceptionManagement.Infrastructure.Persistence.Seeding;

public sealed class SyntheticDataSeeder(TransportationExceptionsDbContext database)
{
    public const int SeedCaseCount = 36;

    private static readonly string[] Nodes =
    [
        "NODE-A",
        "NODE-B",
        "HUB-NORTH",
        "HUB-SOUTH",
        "DC-EAST",
        "DC-WEST"
    ];

    private static readonly string[] Carriers =
    [
        "CARRIER-01",
        "CARRIER-02",
        "CARRIER-03",
        "CARRIER-04"
    ];

    private static readonly string[] Assignees =
    [
        "Analyst-A",
        "Analyst-B",
        "Analyst-C"
    ];

    private static readonly TransportationExceptionType[] ExceptionTypes =
        Enum.GetValues<TransportationExceptionType>();

    private static readonly ExceptionSeverity[] Severities =
        Enum.GetValues<ExceptionSeverity>();

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await database.Cases.AnyAsync(cancellationToken))
        {
            return;
        }

        var cases = Enumerable.Range(1, SeedCaseCount)
            .Select(CreateCase)
            .ToArray();

        await database.Cases.AddRangeAsync(cases, cancellationToken);
        await database.SaveChangesAsync(cancellationToken);
    }

    private static TransportationExceptionCase CreateCase(int number)
    {
        var createdAt = new DateTimeOffset(2026, 1, 15, 8, 0, 0, TimeSpan.Zero)
            .AddHours((number - 1) * 6);
        var severity = Severities[(number - 1) % Severities.Length];
        var statusVariant = (number - 1) % 5;
        var assignee = statusVariant == 0 && number % 2 == 0
            ? null
            : Assignees[(number - 1) % Assignees.Length];
        var dueAt = IllustrativeSlaPolicy.CalculateDueAt(severity, createdAt);

        var entity = TransportationExceptionCase.Create(
            $"CASE-{number:0000}",
            $"MOV-{number:0000}",
            Nodes[(number - 1) % Nodes.Length],
            Nodes[(number + 1) % Nodes.Length],
            Carriers[(number - 1) % Carriers.Length],
            ExceptionTypes[(number - 1) % ExceptionTypes.Length],
            severity,
            $"Synthetic transportation exception example {number:00}.",
            assignee,
            createdAt,
            dueAt);

        if (number % 3 == 0)
        {
            entity.AddNote(
                Assignees[number % Assignees.Length],
                $"Synthetic context note for case {number:00}.",
                createdAt.AddMinutes(5));
        }

        if (statusVariant == 0)
        {
            return entity;
        }

        entity.Assign(assignee!, createdAt.AddMinutes(10));
        entity.TransitionTo(CaseStatus.InProgress, null, createdAt.AddMinutes(15));

        if (statusVariant == 1)
        {
            return entity;
        }

        if (statusVariant == 2)
        {
            entity.TransitionTo(CaseStatus.WaitingExternal, null, createdAt.AddMinutes(30));
            return entity;
        }

        var resolvedAt = number % 2 == 0
            ? dueAt.AddMinutes(-15)
            : dueAt.AddHours(1);
        entity.TransitionTo(
            CaseStatus.Resolved,
            $"Synthetic resolution example for case {number:00}.",
            resolvedAt);

        if (statusVariant == 4)
        {
            entity.TransitionTo(CaseStatus.Closed, null, resolvedAt.AddMinutes(30));
        }

        return entity;
    }
}
