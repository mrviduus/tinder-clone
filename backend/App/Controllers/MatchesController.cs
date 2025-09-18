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
public class MatchesController : ControllerBase
{
    private readonly MatchService _matchService;
    private readonly MessageService _messageService;

    public MatchesController(MatchService matchService, MessageService messageService)
    {
        _matchService = matchService;
        _messageService = messageService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetMatches(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (pageSize > 50) pageSize = 50;

        var userId = GetCurrentUserId();
        var matches = await _matchService.GetMatchesAsync(userId, page, pageSize);

        return Ok(matches);
    }

    [HttpGet("{matchId}")]
    public async Task<IActionResult> GetMatchDetails(Guid matchId)
    {
        var userId = GetCurrentUserId();
        var match = await _matchService.GetMatchDetailsAsync(matchId, userId);

        if (match == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Match not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(match);
    }

    [HttpGet("{matchId}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid matchId,
        [FromQuery] DateTime? before = null,
        [FromQuery] int take = 30)
    {
        if (take > 100) take = 100;

        var userId = GetCurrentUserId();
        var messages = await _messageService.GetMessagesAsync(matchId, userId, before, take);

        return Ok(messages);
    }

    [HttpPost("{matchId}/messages")]
    public async Task<IActionResult> SendMessage(Guid matchId, [FromBody] SendMessageRequest request)
    {
        // Override matchId from route
        request.MatchId = matchId;

        var userId = GetCurrentUserId();
        var message = await _messageService.SendMessageAsync(userId, request);

        if (message == null)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Unable to send message",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Created($"/api/matches/{matchId}/messages", message);
    }
}