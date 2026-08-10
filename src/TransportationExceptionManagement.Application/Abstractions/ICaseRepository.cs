using TransportationExceptionManagement.Application.Cases.Models;
using TransportationExceptionManagement.Application.Common;
using TransportationExceptionManagement.Application.Reports.Dtos;
using TransportationExceptionManagement.Domain.Entities;

namespace TransportationExceptionManagement.Application.Abstractions;

public interface ICaseRepository
{
    Task<PagedResult<TransportationExceptionCase>> SearchAsync(
        CaseQuery query,
        CancellationToken cancellationToken);

    Task<TransportationExceptionCase?> GetReadOnlyAsync(
        int id,
        bool includeNotes,
        CancellationToken cancellationToken);

    Task<TransportationExceptionCase?> GetForUpdateAsync(
        int id,
        CancellationToken cancellationToken);

    Task<bool> CaseReferenceExistsAsync(
        string caseReference,
        CancellationToken cancellationToken);

    Task AddAsync(
        TransportationExceptionCase entity,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TransportationExceptionCase>> ListForExportAsync(
        CaseFilter filter,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CaseReportSnapshot>> ListReportSnapshotsAsync(
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
