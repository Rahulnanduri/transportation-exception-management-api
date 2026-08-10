namespace TransportationExceptionManagement.Application.Exceptions;

public sealed class CaseNotFoundException(int id)
    : Exception($"Transportation exception case {id} was not found.")
{
    public int CaseId { get; } = id;
}
