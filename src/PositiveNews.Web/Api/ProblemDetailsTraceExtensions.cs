using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PositiveNews.Web.Api;

internal static class ProblemDetailsTraceExtensions
{
    internal const string TraceIdExtensionKey = "traceId";
    internal const string TraceTimestampExtensionKey = "timestamp";

    public static void EnrichWithTrace(HttpContext httpContext, ProblemDetails problemDetails)
    {
        problemDetails.Extensions[TraceIdExtensionKey] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        problemDetails.Extensions[TraceTimestampExtensionKey] = DateTime.UtcNow;
    }
}
