using TransportationExceptionManagement.Domain.Enums;
using TransportationExceptionManagement.Domain.Exceptions;
using TransportationExceptionManagement.Domain.Policies;
using TransportationExceptionManagement.Domain.Validation;

namespace TransportationExceptionManagement.Domain.Entities;

public sealed class TransportationExceptionCase
{
    private readonly List<CaseNote> _notes = [];

    private TransportationExceptionCase()
    {
    }

    public TransportationExceptionCase(
        string caseReference,
        string movementReference,
        string originNode,
        string destinationNode,
        string carrierCode,
        TransportationExceptionType exceptionType,
        ExceptionSeverity severity,
        string description,
        DateTimeOffset createdAtUtc,
        string? assignee = null)
    {
        EnsureDefined(exceptionType, nameof(exceptionType));
        EnsureDefined(severity, nameof(severity));

        CaseReference = NormalizeRequired(
            caseReference,
            CaseFieldLimits.CaseReferenceMaxLength,
            nameof(caseReference));
        MovementReference = NormalizeRequired(
            movementReference,
            CaseFieldLimits.MovementReferenceMaxLength,
            nameof(movementReference));
        OriginNode = NormalizeRequired(originNode, CaseFieldLimits.NodeMaxLength, nameof(originNode));
        DestinationNode = NormalizeRequired(
            destinationNode,
            CaseFieldLimits.NodeMaxLength,
            nameof(destinationNode));

        if (string.Equals(OriginNode, DestinationNode, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Origin and destination nodes must be different.",
                nameof(destinationNode));
        }

        CarrierCode = NormalizeRequired(
            carrierCode,
            CaseFieldLimits.CarrierCodeMaxLength,
            nameof(carrierCode));
        Description = NormalizeRequired(
            description,
            CaseFieldLimits.DescriptionMaxLength,
            nameof(description));
        Assignee = NormalizeOptional(assignee, CaseFieldLimits.AssigneeMaxLength, nameof(assignee));
        ExceptionType = exceptionType;
        Severity = severity;
        Status = CaseStatus.New;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        UpdatedAtUtc = CreatedAtUtc;
        DueAtUtc = IllustrativeSlaPolicy.CalculateDueAtUtc(CreatedAtUtc, Severity);
    }

    public static TransportationExceptionCase Create(
        string caseReference,
        string movementReference,
        string originNode,
        string destinationNode,
        string carrierCode,
        TransportationExceptionType exceptionType,
        ExceptionSeverity severity,
        string description,
        string? assignee,
        DateTimeOffset createdAtUtc,
        DateTimeOffset dueAtUtc)
    {
        var entity = new TransportationExceptionCase(
            caseReference,
            movementReference,
            originNode,
            destinationNode,
            carrierCode,
            exceptionType,
            severity,
            description,
            createdAtUtc,
            assignee);

        if (dueAtUtc.ToUniversalTime() != entity.DueAtUtc)
        {
            throw new ArgumentException(
                "Due time must match the illustrative SLA threshold for the selected severity.",
                nameof(dueAtUtc));
        }

        return entity;
    }

    public int Id { get; private set; }

    public string CaseReference { get; private set; } = string.Empty;

    public string MovementReference { get; private set; } = string.Empty;

    public string OriginNode { get; private set; } = string.Empty;

    public string DestinationNode { get; private set; } = string.Empty;

    public string CarrierCode { get; private set; } = string.Empty;

    public TransportationExceptionType ExceptionType { get; private set; }

    public ExceptionSeverity Severity { get; private set; }

    public CaseStatus Status { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public string? Assignee { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public DateTimeOffset DueAtUtc { get; private set; }

    public DateTimeOffset? ResolvedAtUtc { get; private set; }

    public string? ResolutionSummary { get; private set; }

    public IReadOnlyCollection<CaseNote> Notes => _notes;

    public void Assign(string assignee, DateTimeOffset assignedAtUtc)
    {
        DateTimeOffset occurrenceUtc = ValidateOccurrence(assignedAtUtc, nameof(assignedAtUtc));
        Assignee = NormalizeRequired(
            assignee,
            CaseFieldLimits.AssigneeMaxLength,
            nameof(assignee));
        UpdatedAtUtc = occurrenceUtc;
    }

    public void ChangeStatus(
        CaseStatus requestedStatus,
        string? resolutionSummary,
        DateTimeOffset changedAtUtc)
    {
        EnsureDefined(requestedStatus, nameof(requestedStatus));

        if (!IsTransitionAllowed(Status, requestedStatus))
        {
            throw new CaseWorkflowException(
                "invalid_transition",
                $"Transition from {Status} to {requestedStatus} is not allowed.",
                Status,
                requestedStatus);
        }

        if (requestedStatus == CaseStatus.InProgress && string.IsNullOrWhiteSpace(Assignee))
        {
            throw new CaseWorkflowException(
                "assignee_required",
                "A case must have an assignee before moving to InProgress.",
                Status,
                requestedStatus);
        }

        string? normalizedResolution = null;
        if (requestedStatus == CaseStatus.Resolved)
        {
            if (string.IsNullOrWhiteSpace(resolutionSummary))
            {
                throw new CaseWorkflowException(
                    "resolution_summary_required",
                    "A non-empty resolution summary is required when resolving a case.",
                    Status,
                    requestedStatus);
            }

            normalizedResolution = NormalizeRequired(
                resolutionSummary,
                CaseFieldLimits.ResolutionSummaryMaxLength,
                nameof(resolutionSummary));
        }

        DateTimeOffset occurrenceUtc = ValidateOccurrence(changedAtUtc, nameof(changedAtUtc));

        if (requestedStatus == CaseStatus.Resolved)
        {
            ResolutionSummary = normalizedResolution;
            ResolvedAtUtc = occurrenceUtc;
        }
        else if (requestedStatus == CaseStatus.InProgress &&
                 Status is CaseStatus.Resolved or CaseStatus.Closed)
        {
            ResolutionSummary = null;
            ResolvedAtUtc = null;
        }

        Status = requestedStatus;
        UpdatedAtUtc = occurrenceUtc;
    }

    public void TransitionTo(
        CaseStatus requestedStatus,
        string? resolutionSummary,
        DateTimeOffset changedAtUtc) =>
        ChangeStatus(requestedStatus, resolutionSummary, changedAtUtc);

    public CaseNote AddNote(string author, string text, DateTimeOffset createdAtUtc)
    {
        DateTimeOffset occurrenceUtc = ValidateOccurrence(createdAtUtc, nameof(createdAtUtc));
        var note = new CaseNote(author, text, occurrenceUtc);

        _notes.Add(note);
        UpdatedAtUtc = occurrenceUtc;

        return note;
    }

    private static bool IsTransitionAllowed(CaseStatus currentStatus, CaseStatus requestedStatus) =>
        (currentStatus, requestedStatus) switch
        {
            (CaseStatus.New, CaseStatus.InProgress) => true,
            (CaseStatus.InProgress, CaseStatus.WaitingExternal or CaseStatus.Resolved) => true,
            (CaseStatus.WaitingExternal, CaseStatus.InProgress or CaseStatus.Resolved) => true,
            (CaseStatus.Resolved, CaseStatus.Closed or CaseStatus.InProgress) => true,
            (CaseStatus.Closed, CaseStatus.InProgress) => true,
            _ => false,
        };

    private DateTimeOffset ValidateOccurrence(DateTimeOffset occurrence, string parameterName)
    {
        DateTimeOffset occurrenceUtc = occurrence.ToUniversalTime();
        if (occurrenceUtc < UpdatedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                occurrence,
                "Lifecycle timestamps cannot move backwards.");
        }

        return occurrenceUtc;
    }

    private static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return NormalizeRequired(value, maxLength, parameterName);
    }

    private static void EnsureDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown enum value.");
        }
    }
}
