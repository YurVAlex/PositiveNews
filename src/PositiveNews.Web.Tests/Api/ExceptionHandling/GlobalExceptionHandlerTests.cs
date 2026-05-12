using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PositiveNews.Domain.Exceptions;
using PositiveNews.Web.Api.ExceptionHandling;

namespace PositiveNews.Web.Tests.Api.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Should_WriteValidationProblemDetails_When_ValidationException()
    {
        var failures = new[]
        {
            new ValidationFailure("Email", "Bad email") { PropertyName = "Email" }
        };
        var vex = new ValidationException(failures);
        ProblemDetailsContext? captured = null;

        var problemService = Substitute.For<IProblemDetailsService>();
        problemService
            .WriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c))
            .Returns(ValueTask.CompletedTask);

        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(Environments.Production);

        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var sut = new GlobalExceptionHandler(problemService, env, logger);

        var httpContext = new DefaultHttpContext();

        var handled = await sut.TryHandleAsync(httpContext, vex, CancellationToken.None);

        handled.Should().BeTrue();
        await problemService.Received(1).WriteAsync(Arg.Any<ProblemDetailsContext>());
        captured.Should().NotBeNull();
        captured!.ProblemDetails.Should().BeOfType<ValidationProblemDetails>();
        var validationPd = (ValidationProblemDetails)captured.ProblemDetails;
        validationPd.Status.Should().Be(StatusCodes.Status400BadRequest);
        validationPd.Title.Should().Be("One or more validation errors occurred.");
        validationPd.Detail.Should().Be("See the errors field for details.");
        validationPd.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task TryHandleAsync_Should_WriteDomainProblemDetails_When_DomainException()
    {
        ProblemDetailsContext? captured = null;
        var problemService = Substitute.For<IProblemDetailsService>();
        problemService
            .WriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c))
            .Returns(ValueTask.CompletedTask);

        var env = Substitute.For<IHostEnvironment>();
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var sut = new GlobalExceptionHandler(problemService, env, logger);

        var httpContext = new DefaultHttpContext();

        var handled = await sut.TryHandleAsync(httpContext, new DomainException("Rule violated."), CancellationToken.None);

        handled.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.ProblemDetails.Title.Should().Be("Bad Request");
        captured.ProblemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        captured.ProblemDetails.Detail.Should().Be("Rule violated.");
        captured.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
    }

    [Fact]
    public async Task TryHandleAsync_Should_UseGenericDetail_When_UnexpectedAndProduction()
    {
        ProblemDetailsContext? captured = null;
        var problemService = Substitute.For<IProblemDetailsService>();
        problemService
            .WriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c))
            .Returns(ValueTask.CompletedTask);

        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(Environments.Production);

        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var sut = new GlobalExceptionHandler(problemService, env, logger);

        var httpContext = new DefaultHttpContext();

        var handled = await sut.TryHandleAsync(httpContext, new InvalidOperationException("secret"), CancellationToken.None);

        handled.Should().BeTrue();
        captured!.ProblemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        captured.ProblemDetails.Detail.Should().Be("An unexpected error occurred.");
    }

    [Fact]
    public async Task TryHandleAsync_Should_ExposeExceptionMessage_When_UnexpectedAndDevelopment()
    {
        ProblemDetailsContext? captured = null;
        var problemService = Substitute.For<IProblemDetailsService>();
        problemService
            .WriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c))
            .Returns(ValueTask.CompletedTask);

        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(Environments.Development);

        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var sut = new GlobalExceptionHandler(problemService, env, logger);

        var httpContext = new DefaultHttpContext();

        await sut.TryHandleAsync(httpContext, new InvalidOperationException("dev-visible"), CancellationToken.None);

        captured!.ProblemDetails.Detail.Should().Be("dev-visible");
    }

}
