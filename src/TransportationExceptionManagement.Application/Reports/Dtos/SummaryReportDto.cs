namespace TransportationExceptionManagement.Application.Reports.Dtos;

public sealed record SummaryReportDto(
    DateTimeOffset GeneratedAtUtc,
    int TotalCases,
    int ActiveCases,
    int ResolvedCases,
    int ClosedCases,
    int OverdueActiveCases,
    IReadOnlyDictionary<string, int> CountsByStatus,
    IReadOnlyDictionary<string, int> CountsBySeverity,
    IReadOnlyDictionary<string, int> CountsByExceptionType,
    IReadOnlyDictionary<string, int> CountsByAssignee);
