using TransportationExceptionManagement.Application.Abstractions;
using TransportationExceptionManagement.Application.Reports.Dtos;
using TransportationExceptionManagement.Domain.Enums;
using TransportationExceptionManagement.Domain.Policies;

namespace TransportationExceptionManagement.Application.Reports;

public sealed class ReportService(ICaseRepository repository, TimeProvider timeProvider) : IReportService
{
    private const string SlaDisclaimer =
        "Illustrative SLA thresholds use deterministic synthetic portfolio rules and are not operational standards.";

    public async Task<SummaryReportDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var cases = await repository.ListReportSnapshotsAsync(cancellationToken);
        var active = cases.Where(item => IsActive(item.Status)).ToArray();

        return new SummaryReportDto(
            now,
            cases.Count,
            active.Length,
            cases.Count(item => item.ResolvedAtUtc.HasValue),
            cases.Count(item => item.Status == CaseStatus.Closed),
            active.Count(item => item.DueAtUtc < now),
            GroupByName(cases, item => item.Status.ToString()),
            GroupByName(cases, item => item.Severity.ToString()),
            GroupByName(cases, item => item.ExceptionType.ToString()),
            GroupByName(cases, item => string.IsNullOrWhiteSpace(item.Assignee) ? "Unassigned" : item.Assignee));
    }

    public async Task<SlaReportDto> GetSlaAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var cases = await repository.ListReportSnapshotsAsync(cancellationToken);
        var resolved = cases.Where(item => item.ResolvedAtUtc.HasValue).ToArray();
        var within = resolved.Count(item => item.ResolvedAtUtc <= item.DueAtUtc);
        var after = resolved.Length - within;
        decimal? compliance = resolved.Length == 0
            ? null
            : Math.Round(within * 100m / resolved.Length, 2);
        decimal? averageHours = resolved.Length == 0
            ? null
            : Math.Round(
                (decimal)resolved.Average(item => (item.ResolvedAtUtc!.Value - item.CreatedAtUtc).TotalHours),
                2);

        return new SlaReportDto(
            now,
            within,
            after,
            cases.Count(item => IsActive(item.Status) && item.DueAtUtc < now),
            compliance,
            averageHours,
            Enum.GetValues<ExceptionSeverity>().ToDictionary(
                severity => severity.ToString(),
                severity => (int)IllustrativeSlaPolicy.GetTarget(severity).TotalHours),
            SlaDisclaimer);
    }

    private static IReadOnlyDictionary<string, int> GroupByName(
        IEnumerable<CaseReportSnapshot> cases,
        Func<CaseReportSnapshot, string> selector) =>
        cases
            .GroupBy(selector, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

    private static bool IsActive(CaseStatus status) =>
        status is CaseStatus.New or CaseStatus.InProgress or CaseStatus.WaitingExternal;
}
