using TransportationExceptionManagement.Application.Cases.Dtos;
using TransportationExceptionManagement.Domain.Enums;

namespace TransportationExceptionManagement.Tests.Support;

internal static class TestCaseRequests
{
    public static CreateCaseRequest Create(
        string caseReference,
        string? assignee = null,
        ExceptionSeverity severity = ExceptionSeverity.High) =>
        new()
        {
            CaseReference = caseReference,
            MovementReference = $"MOV-{caseReference}",
            OriginNode = "TEST-NODE-A",
            DestinationNode = "TEST-NODE-B",
            CarrierCode = "CARRIER-TEST",
            ExceptionType = TransportationExceptionType.RouteDisruption,
            Severity = severity,
            Description = "Deterministic synthetic integration-test case.",
            Assignee = assignee,
        };
}
