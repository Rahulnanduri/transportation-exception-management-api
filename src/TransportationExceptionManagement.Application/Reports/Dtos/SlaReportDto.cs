namespace TransportationExceptionManagement.Application.Reports.Dtos;

public sealed record SlaReportDto(
    DateTimeOffset GeneratedAtUtc,
    int ResolvedWithinIllustrativeSla,
    int ResolvedAfterIllustrativeSla,
    int CurrentlyOverdue,
    decimal? CompliancePercentage,
    decimal? AverageResolutionHours,
    IReadOnlyDictionary<string, int> IllustrativeThresholdHours,
    string Disclaimer);
