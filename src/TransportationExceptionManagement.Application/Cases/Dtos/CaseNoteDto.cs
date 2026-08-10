namespace TransportationExceptionManagement.Application.Cases.Dtos;

public sealed record CaseNoteDto(
    int Id,
    string Author,
    string Text,
    DateTimeOffset CreatedAtUtc);
