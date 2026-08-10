using TransportationExceptionManagement.Domain.Enums;

namespace TransportationExceptionManagement.Application.Cases.Dtos;

public sealed record CaseListItemDto(
    int Id,
    string CaseReference,
    string MovementReference,
    string OriginNode,
    string DestinationNode,
    string CarrierCode,
    TransportationExceptionType ExceptionType,
    ExceptionSeverity Severity,
    CaseStatus Status,
    string? Assignee,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset DueAtUtc,
    DateTimeOffset? ResolvedAtUtc);
