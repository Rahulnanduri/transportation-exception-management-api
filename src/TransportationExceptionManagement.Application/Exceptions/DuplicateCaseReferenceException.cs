namespace TransportationExceptionManagement.Application.Exceptions;

public sealed class DuplicateCaseReferenceException(string caseReference)
    : Exception($"Case reference '{caseReference}' already exists.")
{
    public string CaseReference { get; } = caseReference;
}
