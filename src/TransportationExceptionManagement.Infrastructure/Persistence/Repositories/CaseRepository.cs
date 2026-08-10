using Microsoft.EntityFrameworkCore;
using TransportationExceptionManagement.Application.Abstractions;
using TransportationExceptionManagement.Application.Cases.Models;
using TransportationExceptionManagement.Application.Common;
using TransportationExceptionManagement.Application.Reports.Dtos;
using TransportationExceptionManagement.Domain.Entities;

namespace TransportationExceptionManagement.Infrastructure.Persistence.Repositories;

public sealed class CaseRepository(TransportationExceptionsDbContext database) : ICaseRepository
{
    public async Task<PagedResult<TransportationExceptionCase>> SearchAsync(
        CaseQuery query,
        CancellationToken cancellationToken)
    {
        var source = ApplyFilters(database.Cases.AsNoTracking(), query);
        var totalCount = await source.CountAsync(cancellationToken);
        var ordered = ApplyOrdering(source, query);
        var items = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TransportationExceptionCase>(items, totalCount);
    }

    public async Task<TransportationExceptionCase?> GetReadOnlyAsync(
        int id,
        bool includeNotes,
        CancellationToken cancellationToken)
    {
        IQueryable<TransportationExceptionCase> query = database.Cases.AsNoTracking();
        if (includeNotes)
        {
            query = query.Include(entity => entity.Notes);
        }

        return await query.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<TransportationExceptionCase?> GetForUpdateAsync(
        int id,
        CancellationToken cancellationToken) =>
        database.Cases
            .Include(entity => entity.Notes)
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public Task<bool> CaseReferenceExistsAsync(
        string caseReference,
        CancellationToken cancellationToken) =>
        database.Cases.AnyAsync(
            entity => entity.CaseReference == caseReference,
            cancellationToken);

    public async Task AddAsync(
        TransportationExceptionCase entity,
        CancellationToken cancellationToken) =>
        await database.Cases.AddAsync(entity, cancellationToken);

    public async Task<IReadOnlyList<TransportationExceptionCase>> ListForExportAsync(
        CaseFilter filter,
        CancellationToken cancellationToken) =>
        await ApplyFilters(database.Cases.AsNoTracking(), filter)
            .OrderBy(entity => entity.CaseReference)
            .ThenBy(entity => entity.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CaseReportSnapshot>> ListReportSnapshotsAsync(
        CancellationToken cancellationToken) =>
        await database.Cases
            .AsNoTracking()
            .Select(entity => new CaseReportSnapshot(
                entity.Status,
                entity.Severity,
                entity.ExceptionType,
                entity.Assignee,
                entity.CreatedAtUtc,
                entity.DueAtUtc,
                entity.ResolvedAtUtc))
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await database.SaveChangesAsync(cancellationToken);

    private static IQueryable<TransportationExceptionCase> ApplyFilters(
        IQueryable<TransportationExceptionCase> query,
        CaseFilter filter)
    {
        if (filter.Status.HasValue)
        {
            query = query.Where(entity => entity.Status == filter.Status.Value);
        }

        if (filter.Severity.HasValue)
        {
            query = query.Where(entity => entity.Severity == filter.Severity.Value);
        }

        if (filter.ExceptionType.HasValue)
        {
            query = query.Where(entity => entity.ExceptionType == filter.ExceptionType.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Assignee))
        {
            var assignee = filter.Assignee.Trim();
            query = query.Where(entity => entity.Assignee == assignee);
        }

        if (!string.IsNullOrWhiteSpace(filter.Origin))
        {
            var originPattern = $"%{filter.Origin.Trim()}%";
            query = query.Where(entity => EF.Functions.Like(entity.OriginNode, originPattern));
        }

        if (!string.IsNullOrWhiteSpace(filter.Destination))
        {
            var destinationPattern = $"%{filter.Destination.Trim()}%";
            query = query.Where(entity => EF.Functions.Like(entity.DestinationNode, destinationPattern));
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(entity => entity.CreatedAtUtc >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(entity => entity.CreatedAtUtc <= filter.CreatedTo.Value);
        }

        return query;
    }

    private static IOrderedQueryable<TransportationExceptionCase> ApplyOrdering(
        IQueryable<TransportationExceptionCase> query,
        CaseQuery parameters)
    {
        var descending = parameters.SortDirection == SortDirection.Desc;

        IOrderedQueryable<TransportationExceptionCase> ordered = parameters.SortBy switch
        {
            CaseSortField.UpdatedAtUtc => descending
                ? query.OrderByDescending(entity => entity.UpdatedAtUtc)
                : query.OrderBy(entity => entity.UpdatedAtUtc),
            CaseSortField.DueAtUtc => descending
                ? query.OrderByDescending(entity => entity.DueAtUtc)
                : query.OrderBy(entity => entity.DueAtUtc),
            CaseSortField.Severity => descending
                ? query.OrderByDescending(entity => entity.Severity)
                : query.OrderBy(entity => entity.Severity),
            CaseSortField.Status => descending
                ? query.OrderByDescending(entity => entity.Status)
                : query.OrderBy(entity => entity.Status),
            CaseSortField.CaseReference => descending
                ? query.OrderByDescending(entity => entity.CaseReference)
                : query.OrderBy(entity => entity.CaseReference),
            _ => descending
                ? query.OrderByDescending(entity => entity.CreatedAtUtc)
                : query.OrderBy(entity => entity.CreatedAtUtc)
        };

        return ordered.ThenBy(entity => entity.Id);
    }
}
