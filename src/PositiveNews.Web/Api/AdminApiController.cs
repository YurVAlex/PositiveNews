using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PositiveNews.Web.Api;

/// <summary>
/// Administrative endpoints restricted to users in the Admin role.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminApiController : ControllerBase
{
    /// <summary>
    /// Simple health-style endpoint confirming administrative access.
    /// </summary>
    /// <returns>JSON indicating that admin authorization succeeded.</returns>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        return Ok(new { ok = true, message = "Admin access granted." });
    }
}
