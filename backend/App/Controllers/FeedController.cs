using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using App.DTOs;
using App.Services;

namespace App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class FeedController : ControllerBase
{
    private readonly FeedService _feedService;

    public FeedController(FeedService feedService)
    {
        _feedService = feedService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetCandidates(
        [FromQuery] int radiusKm = 50,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageSize > 50) pageSize = 50;

        var userId = GetCurrentUserId();
        var candidates = await _feedService.GetCandidatesAsync(userId, radiusKm, page, pageSize);

        return Ok(candidates);
    }
}