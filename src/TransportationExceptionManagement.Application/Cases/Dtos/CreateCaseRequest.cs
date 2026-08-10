using System.ComponentModel.DataAnnotations;
using TransportationExceptionManagement.Domain.Enums;
using TransportationExceptionManagement.Domain.Validation;

namespace TransportationExceptionManagement.Application.Cases.Dtos;

public sealed class CreateCaseRequest : IValidatableObject
{
    [Required]
    [StringLength(CaseFieldLimits.CaseReferenceMaxLength)]
    public string? CaseReference { get; init; }

    [Required]
    [StringLength(CaseFieldLimits.MovementReferenceMaxLength)]
    public string? MovementReference { get; init; }

    [Required]
    [StringLength(CaseFieldLimits.NodeMaxLength)]
    public string? OriginNode { get; init; }

    [Required]
    [StringLength(CaseFieldLimits.NodeMaxLength)]
    public string? DestinationNode { get; init; }

    [Required]
    [StringLength(CaseFieldLimits.CarrierCodeMaxLength)]
    public string? CarrierCode { get; init; }

    [Required]
    public TransportationExceptionType? ExceptionType { get; init; }

    [Required]
    public ExceptionSeverity? Severity { get; init; }

    [Required]
    [StringLength(CaseFieldLimits.DescriptionMaxLength)]
    public string? Description { get; init; }

    [StringLength(CaseFieldLimits.AssigneeMaxLength)]
    public string? Assignee { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(OriginNode)
            && !string.IsNullOrWhiteSpace(DestinationNode)
            && string.Equals(OriginNode.Trim(), DestinationNode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Origin and destination must be different.",
                [nameof(OriginNode), nameof(DestinationNode)]);
        }
    }
}
