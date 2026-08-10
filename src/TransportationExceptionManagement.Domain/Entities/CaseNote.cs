using TransportationExceptionManagement.Domain.Validation;

namespace TransportationExceptionManagement.Domain.Entities;

public sealed class CaseNote
{
    private CaseNote()
    {
    }

    internal CaseNote(string author, string text, DateTimeOffset createdAtUtc)
    {
        Author = NormalizeRequired(author, CaseFieldLimits.NoteAuthorMaxLength, nameof(author));
        Text = NormalizeRequired(text, CaseFieldLimits.NoteTextMaxLength, nameof(text));
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public int Id { get; private set; }

    public int TransportationExceptionCaseId { get; private set; }

    public string Author { get; private set; } = string.Empty;

    public string Text { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

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
}
