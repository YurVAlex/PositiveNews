using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Web.Api;

namespace PositiveNews.Web.Tests.Api.Extensions;

public class ProblemDetailsTraceExtensionsTests
{
    [Fact]
    public void EnrichWithTrace_Should_UseHttpContextTraceIdentifier_When_ActivityCurrentNull()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "manual-trace-id";
        var problem = new ProblemDetails();

        ProblemDetailsTraceExtensions.EnrichWithTrace(httpContext, problem);

        problem.Extensions[ProblemDetailsTraceExtensions.TraceIdExtensionKey].Should().Be("manual-trace-id");
        problem.Extensions[ProblemDetailsTraceExtensions.TraceTimestampExtensionKey].Should().BeOfType<DateTime>();
    }

    [Fact]
    public void EnrichWithTrace_Should_NotRemoveExistingExtensions_When_AlreadySet()
    {
        var httpContext = new DefaultHttpContext();
        var problem = new ProblemDetails();
        problem.Extensions["custom"] = "keep";

        ProblemDetailsTraceExtensions.EnrichWithTrace(httpContext, problem);

        problem.Extensions["custom"].Should().Be("keep");
        problem.Extensions.Should().ContainKey(ProblemDetailsTraceExtensions.TraceIdExtensionKey);
    }
}
