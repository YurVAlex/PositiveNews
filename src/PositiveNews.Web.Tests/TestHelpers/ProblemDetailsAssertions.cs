using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Web.Api;

namespace PositiveNews.Web.Tests.TestHelpers;

internal static class ProblemDetailsAssertions
{
    public static void ShouldHaveHttpStatus(this ProblemDetails problemDetails, int expected)
        => problemDetails.Status.Should().Be(expected);

    public static void ShouldContainTraceExtensions(this ProblemDetails problemDetails)
    {
        problemDetails.Extensions.Should().ContainKey(ProblemDetailsTraceExtensions.TraceIdExtensionKey);
        problemDetails.Extensions.Should().ContainKey(ProblemDetailsTraceExtensions.TraceTimestampExtensionKey);
    }
}
