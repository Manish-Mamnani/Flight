using System.Net;
using System.Text.Json;
using CheckInService.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CheckInService.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.Items.TryGetValue("CorrelationId", out var correlationId)
            ? correlationId?.ToString() ?? context.TraceIdentifier
            : context.TraceIdentifier;

        var statusCode = HttpStatusCode.InternalServerError;
        var title = "An error occurred";
        var detail = exception.Message;

        switch (exception)
        {
            case SeatAlreadyAssignedException:
                statusCode = HttpStatusCode.Conflict;
                title = "Conflict";
                break;

            case CheckInWindowClosedException:
            case CheckInException:
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                title = "Bad Request";
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "Not Found";
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Forbidden;
                title = "Forbidden";
                break;

            default:
                _logger.LogError(exception, "Unhandled Exception: {Message} | TraceId: {TraceId}", exception.Message, traceId);
                detail = "An internal server error occurred.";
                break;
        }

        var problemDetails = new
        {
            type = "about:blank",
            title = title,
            status = (int)statusCode,
            detail = detail,
            instance = context.Request.Path.Value,
            traceId = traceId
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}
