using System.ComponentModel.DataAnnotations;
using TransportationExceptionManagement.Domain.Validation;

namespace TransportationExceptionManagement.Application.Cases.Dtos;

public sealed class UpdateAssignmentRequest
{
    [Required]
    [StringLength(CaseFieldLimits.AssigneeMaxLength, MinimumLength = 1)]
    public string? Assignee { get; init; }
}
