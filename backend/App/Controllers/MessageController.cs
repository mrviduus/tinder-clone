using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using App.DTOs;
using App.Services;

namespace App.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessageController(MessageService messageService)
    {
        _messageService = messageService;
    }

    // GET /api/message/{matchId}
    [HttpGet("{matchId:guid}")]
    public async Task<IActionResult> GetMessages(
        Guid matchId,
        [FromQuery] DateTime? before = null,
        [FromQuery] int take = 30)
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

        if (take > 100) take = 100; // Max messages per request

        var messages = await _messageService.GetMessagesAsync(matchId, userId.Value, before, take);
        return Ok(messages);
    }

    // POST /api/message
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
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

        var message = await _messageService.SendMessageAsync(userId.Value, request);

        if (message == null)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to send message. You may not be part of this match or the message content is invalid.",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return CreatedAtAction(nameof(GetMessages), new { matchId = message.MatchId }, message);
    }

    // POST /api/message/read
    [HttpPost("read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkReadRequest request)
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

        var success = await _messageService.MarkMessagesAsReadAsync(userId.Value, request);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "Failed to mark messages as read. You may not be part of this match.",
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