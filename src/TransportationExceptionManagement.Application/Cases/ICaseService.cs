using TransportationExceptionManagement.Application.Cases.Dtos;
using TransportationExceptionManagement.Application.Cases.Models;
using TransportationExceptionManagement.Application.Common;

namespace TransportationExceptionManagement.Application.Cases;

public interface ICaseService
{
    Task<PagedResponse<CaseListItemDto>> SearchAsync(
        CaseQuery query,
        CancellationToken cancellationToken);

    Task<CaseDetailDto> GetAsync(int id, CancellationToken cancellationToken);

    Task<CaseDetailDto> CreateAsync(
        CreateCaseRequest request,
        CancellationToken cancellationToken);

    Task<CaseDetailDto> AssignAsync(
        int id,
        UpdateAssignmentRequest request,
        CancellationToken cancellationToken);

    Task<CaseDetailDto> UpdateStatusAsync(
        int id,
        UpdateStatusRequest request,
        CancellationToken cancellationToken);

    Task<CaseNoteDto> AddNoteAsync(
        int id,
        AddCaseNoteRequest request,
        CancellationToken cancellationToken);
}
