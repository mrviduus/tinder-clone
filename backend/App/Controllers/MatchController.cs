using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using App.DTOs;
using App.Services;

namespace App.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly MatchService _matchService;

    public MatchController(MatchService matchService)
    {
        _matchService = matchService;
    }

    // GET /api/match
    [HttpGet]
    public async Task<IActionResult> GetMatches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "User not authenticated",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        if (pageSize > 100) pageSize = 100; // Max page size

        var matches = await _matchService.GetMatchesAsync(userId.Value, page, pageSize);
        return Ok(matches);
    }

    // GET /api/match/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMatchDetails(Guid id)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "User not authenticated",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var matchDetails = await _matchService.GetMatchDetailsAsync(id, userId.Value);

        if (matchDetails == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Match not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(matchDetails);
    }

    // POST /api/match/{id}/unmatch
    [HttpPost("{id:guid}/unmatch")]
    public async Task<IActionResult> Unmatch(Guid id)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = "User not authenticated",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var success = await _matchService.UnmatchAsync(id, userId.Value);

        if (!success)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Match not found or you are not part of this match",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}