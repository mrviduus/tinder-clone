using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using App.DTOs;
using App.Services;

namespace App.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SwipeController : ControllerBase
{
    private readonly SwipeService _swipeService;

    public SwipeController(SwipeService swipeService)
    {
        _swipeService = swipeService;
    }

    // POST /api/swipe
    [HttpPost]
    public async Task<IActionResult> CreateSwipe([FromBody] SwipeRequest request)
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

        try
        {
            var response = await _swipeService.ProcessSwipeAsync(userId.Value, request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse
            {
                Error = "An error occurred while processing the swipe",
                TraceId = HttpContext.TraceIdentifier
            });
        }
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