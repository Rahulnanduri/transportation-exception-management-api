using TransportationExceptionManagement.Domain.Enums;

namespace TransportationExceptionManagement.Application.Reports.Dtos;

public sealed record CaseReportSnapshot(
    CaseStatus Status,
    ExceptionSeverity Severity,
    TransportationExceptionType ExceptionType,
    string? Assignee,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset DueAtUtc,
    DateTimeOffset? ResolvedAtUtc);
