using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpsFlow.Application.Common.Exceptions;
using OpsFlow.Domain.Common;

namespace OpsFlow.Api.Errors;

public sealed partial class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ApplicationValidationException validationException)
        {
            var validationProblem = new HttpValidationProblemDetails(
                new Dictionary<string, string[]>(validationException.Errors))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = validationException.Message
            };

            validationProblem.Extensions["correlationId"] =
                httpContext.TraceIdentifier;

            return await WriteAsync(
                httpContext,
                validationProblem,
                cancellationToken);
        }

        var (status, title, detail) = exception switch
        {
            ResourceNotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                exception.Message),
            ConflictException or DomainRuleException => (
                StatusCodes.Status409Conflict,
                "Operation conflict",
                exception.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Use the correlation id for support.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            LogUnhandledError(
                logger,
                httpContext.TraceIdentifier,
                exception);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = environment.IsDevelopment() && exception.InnerException is not null
                ? $"{detail} Diagnostic: {exception.InnerException.Message}"
                : detail
        };
        problem.Extensions["correlationId"] = httpContext.TraceIdentifier;

        return await WriteAsync(httpContext, problem, cancellationToken);
    }

    private async ValueTask<bool> WriteAsync(
        HttpContext httpContext,
        ProblemDetails problem,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = problem.Status
            ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem
            });
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "Unhandled API error with correlation id {CorrelationId}.")]
    private static partial void LogUnhandledError(
        ILogger logger,
        string correlationId,
        Exception exception);
}
