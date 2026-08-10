using TransportationExceptionManagement.Application.Cases.Dtos;
using TransportationExceptionManagement.Domain.Entities;

namespace TransportationExceptionManagement.Application.Cases;

internal static class CaseMappings
{
    public static CaseListItemDto ToListItem(this TransportationExceptionCase entity) =>
        new(
            entity.Id,
            entity.CaseReference,
            entity.MovementReference,
            entity.OriginNode,
            entity.DestinationNode,
            entity.CarrierCode,
            entity.ExceptionType,
            entity.Severity,
            entity.Status,
            entity.Assignee,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.DueAtUtc,
            entity.ResolvedAtUtc);

    public static CaseDetailDto ToDetail(this TransportationExceptionCase entity) =>
        new(
            entity.Id,
            entity.CaseReference,
            entity.MovementReference,
            entity.OriginNode,
            entity.DestinationNode,
            entity.CarrierCode,
            entity.ExceptionType,
            entity.Severity,
            entity.Status,
            entity.Description,
            entity.Assignee,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            entity.DueAtUtc,
            entity.ResolvedAtUtc,
            entity.ResolutionSummary,
            entity.Notes
                .OrderBy(note => note.CreatedAtUtc)
                .ThenBy(note => note.Id)
                .Select(note => note.ToDto())
                .ToArray());

    public static CaseNoteDto ToDto(this CaseNote note) =>
        new(note.Id, note.Author, note.Text, note.CreatedAtUtc);
}
