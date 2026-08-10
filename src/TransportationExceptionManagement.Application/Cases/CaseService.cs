using TransportationExceptionManagement.Application.Abstractions;
using TransportationExceptionManagement.Application.Cases.Dtos;
using TransportationExceptionManagement.Application.Cases.Models;
using TransportationExceptionManagement.Application.Common;
using TransportationExceptionManagement.Application.Exceptions;
using TransportationExceptionManagement.Domain.Entities;
using TransportationExceptionManagement.Domain.Policies;

namespace TransportationExceptionManagement.Application.Cases;

public sealed class CaseService(ICaseRepository repository, TimeProvider timeProvider) : ICaseService
{
    public async Task<PagedResponse<CaseListItemDto>> SearchAsync(
        CaseQuery query,
        CancellationToken cancellationToken)
    {
        var result = await repository.SearchAsync(query, cancellationToken);
        var items = result.Items.Select(entity => entity.ToListItem()).ToArray();

        return PagedResponse<CaseListItemDto>.Create(
            items,
            query.Page,
            query.PageSize,
            result.TotalCount);
    }

    public async Task<CaseDetailDto> GetAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetReadOnlyAsync(id, includeNotes: true, cancellationToken)
            ?? throw new CaseNotFoundException(id);

        return entity.ToDetail();
    }

    public async Task<CaseDetailDto> CreateAsync(
        CreateCaseRequest request,
        CancellationToken cancellationToken)
    {
        var caseReference = request.CaseReference!.Trim();
        if (await repository.CaseReferenceExistsAsync(caseReference, cancellationToken))
        {
            throw new DuplicateCaseReferenceException(caseReference);
        }

        var now = timeProvider.GetUtcNow();
        var entity = TransportationExceptionCase.Create(
            caseReference,
            request.MovementReference!,
            request.OriginNode!,
            request.DestinationNode!,
            request.CarrierCode!,
            request.ExceptionType!.Value,
            request.Severity!.Value,
            request.Description!,
            request.Assignee,
            now,
            IllustrativeSlaPolicy.CalculateDueAt(request.Severity.Value, now));

        await repository.AddAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return entity.ToDetail();
    }

    public async Task<CaseDetailDto> AssignAsync(
        int id,
        UpdateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await GetForUpdateAsync(id, cancellationToken);
        entity.Assign(request.Assignee!, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return entity.ToDetail();
    }

    public async Task<CaseDetailDto> UpdateStatusAsync(
        int id,
        UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await GetForUpdateAsync(id, cancellationToken);
        entity.TransitionTo(
            request.Status!.Value,
            request.ResolutionSummary,
            timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return entity.ToDetail();
    }

    public async Task<CaseNoteDto> AddNoteAsync(
        int id,
        AddCaseNoteRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await GetForUpdateAsync(id, cancellationToken);
        var note = entity.AddNote(request.Author!, request.Text!, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return note.ToDto();
    }

    private async Task<TransportationExceptionCase> GetForUpdateAsync(
        int id,
        CancellationToken cancellationToken) =>
        await repository.GetForUpdateAsync(id, cancellationToken)
            ?? throw new CaseNotFoundException(id);
}
