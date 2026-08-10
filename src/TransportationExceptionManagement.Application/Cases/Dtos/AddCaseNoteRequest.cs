using System.ComponentModel.DataAnnotations;
using TransportationExceptionManagement.Domain.Validation;

namespace TransportationExceptionManagement.Application.Cases.Dtos;

public sealed class AddCaseNoteRequest
{
    [Required]
    [StringLength(CaseFieldLimits.NoteAuthorMaxLength, MinimumLength = 1)]
    public string? Author { get; init; }

    [Required]
    [StringLength(CaseFieldLimits.NoteTextMaxLength, MinimumLength = 1)]
    public string? Text { get; init; }
}
