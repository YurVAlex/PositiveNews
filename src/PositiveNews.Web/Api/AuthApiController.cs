using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PositiveNews.Application.Commands.Auth;
using PositiveNews.Application.Queries.Auth;
using PositiveNews.Web.Api.Mapping;
using PositiveNews.Web.Api.Models;

namespace PositiveNews.Web.Api;

/// <summary>
/// HTTP API for user registration, authentication, and current-user profile access.
/// </summary>
/// <param name="mediator">MediatR pipeline for auth commands and queries.</param>
[ApiController]
[Route("api/auth")]
public sealed class AuthApiController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Registers a new user account and returns JWT credentials when successful.
    /// </summary>
    /// <param name="request">Registration payload (email, display name, password).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Authentication tokens and profile, or a validation or conflict problem response.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new RegisterUserCommand(request.Email, request.Name, request.Password), cancellationToken);
        return result
            .Map(auth => auth.ToAuthResponse())
            .ToActionResult(this);
    }

    /// <summary>
    /// Authenticates a user and returns JWT credentials.
    /// </summary>
    /// <param name="request">Login credentials (email and password).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>Authentication tokens and profile, or an error problem response.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new LoginUserCommand(request.Email, request.Password), cancellationToken);
        return result
            .Map(auth => auth.ToAuthResponse())
            .ToActionResult(this);
    }

    /// <summary>
    /// Returns the profile for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The user profile, or unauthorized when the security context is invalid.</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> Me(CancellationToken cancellationToken = default)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdValue, out var userId))
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Invalid or missing user identifier in the security context.",
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
            };
            ProblemDetailsTraceExtensions.EnrichWithTrace(HttpContext, problemDetails);
            return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        }

        var result = await mediator.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return result
            .Map(profile => profile.ToUserProfileResponse())
            .ToActionResult(this);
    }
}
