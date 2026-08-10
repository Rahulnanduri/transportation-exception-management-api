using TransportationExceptionManagement.Domain.Enums;

namespace TransportationExceptionManagement.Application.Cases.Models;

public class CaseFilter
{
    public CaseStatus? Status { get; init; }

    public ExceptionSeverity? Severity { get; init; }

    public TransportationExceptionType? ExceptionType { get; init; }

    public string? Assignee { get; init; }

    public string? Origin { get; init; }

    public string? Destination { get; init; }

    public DateTimeOffset? CreatedFrom { get; init; }

    public DateTimeOffset? CreatedTo { get; init; }
}
