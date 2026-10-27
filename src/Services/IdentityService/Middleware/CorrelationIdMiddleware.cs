using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
            || !int.TryParse(correlationId, out var numericCorrelationId)
            || numericCorrelationId <= 0)
        {
            correlationId = RandomNumberGenerator.GetInt32(1, int.MaxValue).ToString();
        }

        context.Items["CorrelationId"] = correlationId.ToString();

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers.Append(CorrelationIdHeaderName, correlationId.ToString());
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
