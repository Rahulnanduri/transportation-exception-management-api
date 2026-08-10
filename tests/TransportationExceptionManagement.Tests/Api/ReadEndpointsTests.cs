using System.Net;
using System.Net.Http.Json;
using TransportationExceptionManagement.Application.Cases.Dtos;
using TransportationExceptionManagement.Application.Common;
using TransportationExceptionManagement.Application.Reports.Dtos;
using TransportationExceptionManagement.Domain.Enums;
using TransportationExceptionManagement.Tests.Support;

namespace TransportationExceptionManagement.Tests.Api;

public sealed class ReadEndpointsTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsHealthyResponse()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task List_ReturnsAllThirtySixSeededCases()
    {
        var page = await GetPageAsync("/api/cases?pageSize=100");

        Assert.Equal(36, page.TotalCount);
        Assert.Equal(36, page.Items.Count);
        Assert.Equal(1, page.TotalPages);
        Assert.Contains(page.Items, item => item.CaseReference == "CASE-0001");
    }

    [Fact]
    public async Task List_PaginationReturnsDistinctPagesAndMetadata()
    {
        var first = await GetPageAsync("/api/cases?page=1&pageSize=5&sortBy=CaseReference&sortDirection=Asc");
        var second = await GetPageAsync("/api/cases?page=2&pageSize=5&sortBy=CaseReference&sortDirection=Asc");

        Assert.Equal(5, first.Items.Count);
        Assert.Equal(5, second.Items.Count);
        Assert.Equal(8, first.TotalPages);
        Assert.Empty(first.Items.Select(item => item.Id).Intersect(second.Items.Select(item => item.Id)));
    }

    [Fact]
    public async Task List_StatusFilterReturnsOnlyRequestedStatus()
    {
        var page = await GetPageAsync("/api/cases?status=WaitingExternal&pageSize=100");

        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, item => Assert.Equal(CaseStatus.WaitingExternal, item.Status));
    }

    [Fact]
    public async Task List_SeverityFilterReturnsOnlyRequestedSeverity()
    {
        var page = await GetPageAsync("/api/cases?severity=Critical&pageSize=100");

        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, item => Assert.Equal(ExceptionSeverity.Critical, item.Severity));
    }

    [Fact]
    public async Task List_ExceptionTypeFilterReturnsOnlyRequestedType()
    {
        var page = await GetPageAsync("/api/cases?exceptionType=PickupDelay&pageSize=100");

        Assert.NotEmpty(page.Items);
        Assert.All(
            page.Items,
            item => Assert.Equal(TransportationExceptionType.PickupDelay, item.ExceptionType));
    }

    [Fact]
    public async Task List_AssigneeFilterReturnsOnlyRequestedAssignee()
    {
        var page = await GetPageAsync("/api/cases?assignee=Analyst-A&pageSize=100");

        Assert.NotEmpty(page.Items);
        Assert.All(
            page.Items,
            item => Assert.Equal("Analyst-A", item.Assignee));
    }

    [Fact]
    public async Task List_NodeFiltersReturnMatchingCases()
    {
        var page = await GetPageAsync("/api/cases?origin=NODE-A&destination=HUB-NORTH&pageSize=100");

        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, item => Assert.Equal("NODE-A", item.OriginNode));
        Assert.All(page.Items, item => Assert.Equal("HUB-NORTH", item.DestinationNode));
    }

    [Fact]
    public async Task List_CreatedFromFilterIsAppliedInclusively()
    {
        var boundary = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);
        var encoded = Uri.EscapeDataString(boundary.ToString("O"));
        var page = await GetPageAsync($"/api/cases?createdFrom={encoded}&pageSize=100");

        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, item => Assert.True(item.CreatedAtUtc >= boundary));
    }

    [Fact]
    public async Task List_SortByCaseReferenceAscendingIsDeterministic()
    {
        var page = await GetPageAsync(
            "/api/cases?pageSize=100&sortBy=CaseReference&sortDirection=Asc");
        var actual = page.Items.Select(item => item.CaseReference).ToArray();
        var expected = actual.Order(StringComparer.OrdinalIgnoreCase).ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task List_InvalidPaginationReturnsValidationProblem()
    {
        var response = await _client.GetAsync("/api/cases?page=0&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task List_InvalidSortFieldReturnsValidationProblem()
    {
        var response = await _client.GetAsync("/api/cases?sortBy=DropTable");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Detail_ExistingCaseReturnsLifecycleAndNotes()
    {
        var page = await GetPageAsync("/api/cases?exceptionType=CapacityConstraint&pageSize=100");
        var seededWithNote = Assert.Single(page.Items, item => item.CaseReference == "CASE-0003");

        var response = await _client.GetAsync($"/api/cases/{seededWithNote.Id}");
        var detail = await response.ReadJsonAsync<CaseDetailDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("CASE-0003", detail.CaseReference);
        Assert.True(detail.DueAtUtc > detail.CreatedAtUtc);
        Assert.NotEmpty(detail.Notes);
    }

    [Fact]
    public async Task Detail_UnknownCaseReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync("/api/cases/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("case_not_found", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task SummaryReport_ReconcilesSeededCounts()
    {
        var summary = await _client.GetFromJsonAsync<SummaryReportDto>(
            "/api/reports/summary",
            TestHttp.JsonOptions);

        Assert.NotNull(summary);
        Assert.Equal(36, summary.TotalCases);
        Assert.Equal(22, summary.ActiveCases);
        Assert.Equal(14, summary.ResolvedCases);
        Assert.Equal(7, summary.ClosedCases);
        Assert.Equal(36, summary.CountsByStatus.Values.Sum());
        Assert.Equal(36, summary.CountsBySeverity.Values.Sum());
    }

    [Fact]
    public async Task SlaReport_ReturnsSyntheticPolicyMetrics()
    {
        var report = await _client.GetFromJsonAsync<SlaReportDto>(
            "/api/reports/sla",
            TestHttp.JsonOptions);

        Assert.NotNull(report);
        Assert.Equal(7, report.ResolvedWithinIllustrativeSla);
        Assert.Equal(7, report.ResolvedAfterIllustrativeSla);
        Assert.Equal(22, report.CurrentlyOverdue);
        Assert.Equal(50m, report.CompliancePercentage);
        Assert.Equal(2, report.IllustrativeThresholdHours["Critical"]);
        Assert.Contains("synthetic", report.Disclaimer, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CsvExport_ReturnsExpectedContentTypeColumnsAndOrdering()
    {
        var response = await _client.GetAsync("/api/cases/export.csv");
        var csv = await response.Content.ReadAsStringAsync();
        var lines = csv.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(
            "CaseReference,MovementReference,OriginNode,DestinationNode,CarrierCode,ExceptionType,Severity,Status,Assignee,CreatedAtUtc,DueAtUtc,ResolvedAtUtc",
            lines[0]);
        Assert.Equal(37, lines.Length);
        Assert.StartsWith("CASE-0001,", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public async Task CsvExport_StatusFilterReturnsOnlyMatchingRows()
    {
        var csv = await _client.GetStringAsync("/api/cases/export.csv?status=Resolved");
        var rows = csv.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Skip(1);

        Assert.NotEmpty(rows);
        Assert.All(rows, row => Assert.Equal("Resolved", row.Split(',')[7]));
    }

    private async Task<PagedResponse<CaseListItemDto>> GetPageAsync(string path)
    {
        var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadJsonAsync<PagedResponse<CaseListItemDto>>();
    }
}
