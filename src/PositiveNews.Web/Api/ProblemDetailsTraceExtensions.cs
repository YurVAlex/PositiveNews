using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PositiveNews.Web.Api;

/// <summary>
/// Adds correlation and timing metadata to <see cref="ProblemDetails"/> responses.
/// </summary>
internal static class ProblemDetailsTraceExtensions
{
    /// <summary>
    /// Extension key used to store the distributed trace or request identifier.
    /// </summary>
    internal const string TraceIdExtensionKey = "traceId";

    /// <summary>
    /// Extension key used to store the UTC timestamp when the problem details were produced.
    /// </summary>
    internal const string TraceTimestampExtensionKey = "timestamp";

    /// <summary>
    /// Populates standard extensions on <paramref name="problemDetails"/> for diagnostics.
    /// </summary>
    /// <param name="httpContext">The current HTTP context (used for fallback trace identifiers).</param>
    /// <param name="problemDetails">The RFC 7807 problem details instance to enrich.</param>
    /// <remarks>
    /// Uses <see cref="System.Diagnostics.Activity.Current"/> when available; otherwise falls back to
    /// <see cref="HttpContext.TraceIdentifier"/>.
    /// </remarks>
    public static void EnrichWithTrace(HttpContext httpContext, ProblemDetails problemDetails)
    {
        problemDetails.Extensions[TraceIdExtensionKey] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        problemDetails.Extensions[TraceTimestampExtensionKey] = DateTime.UtcNow;
    }
}
