namespace TransportationExceptionManagement.Domain.Validation;

public static class CaseFieldLimits
{
    public const int Reference = 64;
    public const int Node = 64;
    public const int Carrier = 32;
    public const int Description = 1_000;
    public const int Assignee = 100;
    public const int ResolutionSummary = 1_000;
    public const int NoteAuthor = 100;
    public const int NoteText = 2_000;

    public const int CaseReferenceMaxLength = Reference;
    public const int MovementReferenceMaxLength = Reference;
    public const int NodeMaxLength = Node;
    public const int CarrierCodeMaxLength = Carrier;
    public const int DescriptionMaxLength = Description;
    public const int AssigneeMaxLength = Assignee;
    public const int ResolutionSummaryMaxLength = ResolutionSummary;
    public const int NoteAuthorMaxLength = NoteAuthor;
    public const int NoteTextMaxLength = NoteText;
}
