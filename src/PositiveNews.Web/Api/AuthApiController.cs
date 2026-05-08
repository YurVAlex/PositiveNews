using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PositiveNews.Domain.Entities;
using PositiveNews.Infrastructure.Persistence;
using PositiveNews.Web.Api.Models;
using PositiveNews.Web.Security;

namespace PositiveNews.Web.Api;

[ApiController]
[Route("api/auth")]
public sealed class AuthApiController(
    AppDbContext dbContext,
    IJwtTokenService jwtTokenService,
    IOptions<JwtOptions> jwtOptions) : ControllerBase
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var name = request.Name.Trim();
        if (email.Length == 0 || name.Length == 0 || request.Password.Length < 6)
        {
            return BadRequest("Email, name, and password (min 6 chars) are required.");
        }

        var emailExists = await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (emailExists)
        {
            return Conflict("A user with this email already exists.");
        }

        var userRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
        if (userRole is null)
        {
            return Problem("Default 'User' role is missing.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var user = Domain.Entities.User.Create(email, name);
        user.ConfirmEmail();
        user.SetPasswordHash(new PasswordHasher<User>().HashPassword(user, request.Password));
        user.RecordSuccessfulLogin();

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        dbContext.UserRoles.Add(UserRole.Create(user.Id, userRole.Id));
        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = new[] { userRole.Name };
        var token = jwtTokenService.CreateAccessToken(user, roles);

        return Ok(new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            User = new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Roles = roles
            }
        });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (email.Length == 0 || request.Password.Length == 0)
        {
            return Unauthorized();
        }

        var user = await dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return Unauthorized();
        }

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            user.RecordFailedLogin();
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unauthorized();
        }

        user.RecordSuccessfulLogin();
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        var token = jwtTokenService.CreateAccessToken(user, roles);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            User = new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Roles = roles
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> Me(CancellationToken cancellationToken = default)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        return Ok(new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray()
        });
    }
}
