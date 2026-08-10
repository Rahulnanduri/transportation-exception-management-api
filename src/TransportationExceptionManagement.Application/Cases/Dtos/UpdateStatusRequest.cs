using System.ComponentModel.DataAnnotations;
using TransportationExceptionManagement.Domain.Enums;
using TransportationExceptionManagement.Domain.Validation;

namespace TransportationExceptionManagement.Application.Cases.Dtos;

public sealed class UpdateStatusRequest
{
    [Required]
    public CaseStatus? Status { get; init; }

    [StringLength(CaseFieldLimits.ResolutionSummaryMaxLength)]
    public string? ResolutionSummary { get; init; }
}
