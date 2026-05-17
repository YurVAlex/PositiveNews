using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PositiveNews.Application.Common;
using PositiveNews.Web.Api;
using PositiveNews.Web.Tests.TestHelpers;

namespace PositiveNews.Web.Tests.Api.Extensions;

public class ResultExtensionsTests
{
    private sealed class TestController : ControllerBase
    {
        public TestController()
        {
            ControllerContext = ControllerContextFactory.Create();
        }
    }

    [Fact]
    public void ToActionResult_Should_ReturnOkWithPayload_When_GenericSuccess()
    {
        var controller = new TestController();

        var actionResult = Result<string>.Success("payload").ToActionResult(controller);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be("payload");
    }

    [Fact]
    public void ToActionResult_Should_ReturnOkWithoutPayload_When_NonGenericSuccess()
    {
        var controller = new TestController();

        var actionResult = Result.Success().ToActionResult(controller);

        actionResult.Should().BeOfType<OkResult>();
    }

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Unexpected, StatusCodes.Status500InternalServerError)]
    public void ToActionResult_Should_MapErrorTypesToProblemDetails_When_GenericFailure(
        ErrorType errorType,
        int expectedStatusCode)
    {
        var controller = new TestController();
        var error = new Error("Test.Code", "Failure message.", errorType);

        var actionResult = Result<int>.Failure(error).ToActionResult(controller);

        var objectResult = actionResult.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(expectedStatusCode);
        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.ShouldHaveHttpStatus(expectedStatusCode);
        problem.Title.Should().Be("Test.Code");
        problem.Detail.Should().Be("Failure message.");
        problem.ShouldContainTraceExtensions();
    }

    [Fact]
    public void ToActionResult_Should_EnrichProblemDetailsWithTrace_When_GenericFailure()
    {
        var controller = new TestController();
        var ctx = controller.HttpContext;
        ctx.TraceIdentifier = "trace-from-http-context";

        var actionResult = Result<int>.Failure(new Error("X", "msg", ErrorType.Validation)).ToActionResult(controller);

        var problem = actionResult.Result.Should().BeOfType<ObjectResult>().Subject.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Extensions[ProblemDetailsTraceExtensions.TraceIdExtensionKey].Should().NotBeNull();
        problem.Extensions[ProblemDetailsTraceExtensions.TraceTimestampExtensionKey].Should().BeOfType<DateTime>();
    }

    [Fact]
    public void ToActionResult_Should_ThrowInvalidOperationException_When_ErrorTypeNoneForFailure()
    {
        var controller = new TestController();
        var result = Result.Failure(new Error("Invalid", "Invalid", ErrorType.None));

        var act = () => result.ToActionResult(controller);

        act.Should().Throw<InvalidOperationException>();
    }
}
