using System.ComponentModel.DataAnnotations;

namespace TransportationExceptionManagement.Application.Cases.Models;

public sealed class CaseQuery : CaseFilter
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;

    public CaseSortField SortBy { get; init; } = CaseSortField.CreatedAtUtc;

    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}
