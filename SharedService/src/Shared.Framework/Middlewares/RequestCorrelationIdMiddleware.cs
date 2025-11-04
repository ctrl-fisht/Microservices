using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Shared.Framework.Middlewares;

public class RequestCorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly Action<string>? _pushToSerilog;

    public RequestCorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;

        var logContextType = Type.GetType("Serilog.Context.LogContext, Serilog");
        var pushProperty = logContextType?
            .GetMethod("PushProperty", new[] { typeof(string), typeof(object), typeof(bool) });

        if (pushProperty is not null)
        {
            _pushToSerilog = (correlationId) =>
            {
                var disposable = pushProperty.Invoke(null, new object[] { "CorrelationId", correlationId, false }) as IDisposable;
                // disposable можно сохранить в using
                _pushScope = disposable;
            };
        }
    }

    [ThreadStatic]
    private static IDisposable? _pushScope;

    public async Task Invoke(HttpContext context)
    {
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationIdValues);
        var correlationId = correlationIdValues.FirstOrDefault() ?? context.TraceIdentifier;

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        IDisposable? scope = null;
        if (_pushToSerilog is not null)
        {
            var logContextType = Type.GetType("Serilog.Context.LogContext, Serilog");
            var pushProperty = logContextType?.GetMethod("PushProperty", new[] { typeof(string), typeof(object), typeof(bool) });
            scope = pushProperty?.Invoke(null, new object[] { "CorrelationId", correlationId, false }) as IDisposable;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            scope?.Dispose();
        }
    }
}

public static class RequestCorrelationIdMiddlewareExtensions
{
    public static void UseRequestCorrelationId(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestCorrelationIdMiddleware>();
    }
}