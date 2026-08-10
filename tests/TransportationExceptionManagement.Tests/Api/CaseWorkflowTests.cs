using System.Net;
using System.Net.Http.Json;
using TransportationExceptionManagement.Application.Cases.Dtos;
using TransportationExceptionManagement.Domain.Enums;
using TransportationExceptionManagement.Tests.Support;

namespace TransportationExceptionManagement.Tests.Api;

public sealed class CaseWorkflowTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ValidRequestReturnsCreatedLocationAndCalculatedDueTime()
    {
        var (response, created) = await CreateAsync(
            TestCaseRequests.Create("TEST-CREATE-001", severity: ExceptionSeverity.Critical));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.EndsWith(
            $"/api/cases/{created.Id}",
            response.Headers.Location?.OriginalString,
            StringComparison.Ordinal);
        Assert.Equal(CaseStatus.New, created.Status);
        Assert.Equal(TestApiFactory.FixedUtcNow, created.CreatedAtUtc);
        Assert.Equal(TestApiFactory.FixedUtcNow.AddHours(2), created.DueAtUtc);
    }

    [Fact]
    public async Task Create_SameOriginAndDestinationReturnsValidationProblem()
    {
        var request = TestCaseRequests.Create("TEST-SAME-NODE");
        request = new CreateCaseRequest
        {
            CaseReference = request.CaseReference,
            MovementReference = request.MovementReference,
            OriginNode = "SAME-NODE",
            DestinationNode = " same-node ",
            CarrierCode = request.CarrierCode,
            ExceptionType = request.ExceptionType,
            Severity = request.Severity,
            Description = request.Description,
        };

        var response = await _client.PostAsJsonAsync("/api/cases", request, TestHttp.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Create_MissingRequiredFieldsReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/cases",
            new { caseReference = "TEST-INCOMPLETE" },
            TestHttp.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_failed", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Create_UnknownStringEnumReturnsBadRequest()
    {
        const string json =
            """
            {
              "caseReference": "TEST-BAD-ENUM",
              "movementReference": "MOV-BAD-ENUM",
              "originNode": "NODE-A",
              "destinationNode": "NODE-B",
              "carrierCode": "CARRIER-TEST",
              "exceptionType": "UnknownType",
              "severity": "High",
              "description": "Synthetic test."
            }
            """;
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/cases", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DuplicateCaseReferenceReturnsConflictProblem()
    {
        var request = TestCaseRequests.Create("TEST-DUPLICATE");
        await CreateAsync(request);

        var response = await _client.PostAsJsonAsync("/api/cases", request, TestHttp.JsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("duplicate_case_reference", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Assignment_UpdatesAssigneeAndTimestamp()
    {
        var (_, created) = await CreateAsync(TestCaseRequests.Create("TEST-ASSIGN"));

        var response = await _client.PatchAsJsonAsync(
            $"/api/cases/{created.Id}/assignment",
            new UpdateAssignmentRequest { Assignee = "Analyst-Test" },
            TestHttp.JsonOptions);
        var updated = await response.ReadJsonAsync<CaseDetailDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Analyst-Test", updated.Assignee);
        Assert.Equal(TestApiFactory.FixedUtcNow, updated.UpdatedAtUtc);
    }

    [Fact]
    public async Task Assignment_EmptyValueReturnsBadRequest()
    {
        var (_, created) = await CreateAsync(TestCaseRequests.Create("TEST-EMPTY-ASSIGN"));

        var response = await _client.PatchAsJsonAsync(
            $"/api/cases/{created.Id}/assignment",
            new { assignee = "" },
            TestHttp.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Status_UnassignedNewCaseCannotEnterInProgress()
    {
        var (_, created) = await CreateAsync(TestCaseRequests.Create("TEST-UNASSIGNED"));

        var response = await ChangeStatusAsync(created.Id, CaseStatus.InProgress);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("assignee_required", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Status_AssignedNewCaseCanEnterInProgress()
    {
        var (_, created) = await CreateAsync(
            TestCaseRequests.Create("TEST-IN-PROGRESS", "Analyst-Test"));

        var response = await ChangeStatusAsync(created.Id, CaseStatus.InProgress);
        var updated = await response.ReadJsonAsync<CaseDetailDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(CaseStatus.InProgress, updated.Status);
    }

    [Fact]
    public async Task Status_NewCaseCannotResolveDirectly()
    {
        var (_, created) = await CreateAsync(
            TestCaseRequests.Create("TEST-DIRECT-RESOLVE", "Analyst-Test"));

        var response = await ChangeStatusAsync(
            created.Id,
            CaseStatus.Resolved,
            "Synthetic resolution.");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("invalid_transition", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Status_ResolvingWithoutSummaryReturnsConflict()
    {
        var (_, created) = await CreateAsync(
            TestCaseRequests.Create("TEST-NO-SUMMARY", "Analyst-Test"));
        await AssertSuccessfulStatusAsync(created.Id, CaseStatus.InProgress);

        var response = await ChangeStatusAsync(created.Id, CaseStatus.Resolved);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("resolution_summary_required", await response.ReadProblemCodeAsync());
    }

    [Fact]
    public async Task Status_ResolveThenCloseSetsLifecycleFields()
    {
        var (_, created) = await CreateAsync(
            TestCaseRequests.Create("TEST-RESOLVE-CLOSE", "Analyst-Test"));
        await AssertSuccessfulStatusAsync(created.Id, CaseStatus.InProgress);

        var resolved = await AssertSuccessfulStatusAsync(
            created.Id,
            CaseStatus.Resolved,
            "Synthetic resolution completed.");
        var closed = await AssertSuccessfulStatusAsync(created.Id, CaseStatus.Closed);

        Assert.Equal(TestApiFactory.FixedUtcNow, resolved.ResolvedAtUtc);
        Assert.Equal("Synthetic resolution completed.", resolved.ResolutionSummary);
        Assert.Equal(CaseStatus.Closed, closed.Status);
        Assert.Equal(resolved.ResolvedAtUtc, closed.ResolvedAtUtc);
    }

    [Fact]
    public async Task Status_ReopeningResolvedCaseClearsCurrentResolution()
    {
        var (_, created) = await CreateAsync(
            TestCaseRequests.Create("TEST-REOPEN", "Analyst-Test"));
        await AssertSuccessfulStatusAsync(created.Id, CaseStatus.InProgress);
        await AssertSuccessfulStatusAsync(created.Id, CaseStatus.Resolved, "Synthetic resolution.");

        var reopened = await AssertSuccessfulStatusAsync(created.Id, CaseStatus.InProgress);

        Assert.Equal(CaseStatus.InProgress, reopened.Status);
        Assert.Null(reopened.ResolvedAtUtc);
        Assert.Null(reopened.ResolutionSummary);
    }

    [Fact]
    public async Task Notes_ValidNoteIsCreatedAndReturnedInDetail()
    {
        var (_, created) = await CreateAsync(TestCaseRequests.Create("TEST-NOTE"));

        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{created.Id}/notes",
            new AddCaseNoteRequest
            {
                Author = "Analyst-Test",
                Text = "Synthetic integration-test note.",
            },
            TestHttp.JsonOptions);
        var note = await response.ReadJsonAsync<CaseNoteDto>();
        var detail = await _client.GetFromJsonAsync<CaseDetailDto>(
            $"/api/cases/{created.Id}",
            TestHttp.JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(note.Id > 0);
        Assert.NotNull(detail);
        Assert.Contains(detail.Notes, item => item.Id == note.Id && item.Text == note.Text);
    }

    [Fact]
    public async Task Notes_EmptyTextReturnsBadRequest()
    {
        var (_, created) = await CreateAsync(TestCaseRequests.Create("TEST-EMPTY-NOTE"));

        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{created.Id}/notes",
            new { author = "Analyst-Test", text = "" },
            TestHttp.JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Mutation_UnknownCaseReturnsNotFound()
    {
        var response = await _client.PatchAsJsonAsync(
            "/api/cases/999999/assignment",
            new UpdateAssignmentRequest { Assignee = "Analyst-Test" },
            TestHttp.JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("case_not_found", await response.ReadProblemCodeAsync());
    }

    private async Task<(HttpResponseMessage Response, CaseDetailDto Created)> CreateAsync(
        CreateCaseRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/cases", request, TestHttp.JsonOptions);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (response, await response.ReadJsonAsync<CaseDetailDto>());
    }

    private Task<HttpResponseMessage> ChangeStatusAsync(
        int id,
        CaseStatus status,
        string? resolutionSummary = null) =>
        _client.PatchAsJsonAsync(
            $"/api/cases/{id}/status",
            new UpdateStatusRequest
            {
                Status = status,
                ResolutionSummary = resolutionSummary,
            },
            TestHttp.JsonOptions);

    private async Task<CaseDetailDto> AssertSuccessfulStatusAsync(
        int id,
        CaseStatus status,
        string? resolutionSummary = null)
    {
        var response = await ChangeStatusAsync(id, status, resolutionSummary);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadJsonAsync<CaseDetailDto>();
    }
}
