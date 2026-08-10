using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TransportationExceptionManagement.Application.Exceptions;
using TransportationExceptionManagement.Domain.Exceptions;

namespace TransportationExceptionManagement.Api.ErrorHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, code, logLevel) = exception switch
        {
            CaseNotFoundException => (
                StatusCodes.Status404NotFound,
                "Case not found",
                "case_not_found",
                LogLevel.Information),
            DuplicateCaseReferenceException => (
                StatusCodes.Status409Conflict,
                "Duplicate case reference",
                "duplicate_case_reference",
                LogLevel.Information),
            CaseWorkflowException workflowException => (
                StatusCodes.Status409Conflict,
                "Case workflow conflict",
                workflowException.Code,
                LogLevel.Information),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                "invalid_request",
                LogLevel.Information),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected server error",
                "unexpected_error",
                LogLevel.Error)
        };

        logger.Log(
            logLevel,
            exception,
            "Request failed with {ErrorCode} at {Path}",
            code,
            httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Type = $"https://httpstatuses.com/{status}",
            Instance = httpContext.Request.Path
        };

        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
