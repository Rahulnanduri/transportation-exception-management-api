using TransportationExceptionManagement.Domain.Enums;

namespace TransportationExceptionManagement.Domain.Exceptions;

public sealed class CaseWorkflowException : Exception
{
    public CaseWorkflowException(
        string code,
        string message,
        CaseStatus currentStatus,
        CaseStatus requestedStatus)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Code = code;
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }

    public string Code { get; }

    public CaseStatus CurrentStatus { get; }

    public CaseStatus RequestedStatus { get; }
}
