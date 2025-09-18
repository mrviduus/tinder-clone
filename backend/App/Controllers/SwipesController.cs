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
public class SwipesController : ControllerBase
{
    private readonly SwipeService _swipeService;

    public SwipesController(SwipeService swipeService)
    {
        _swipeService = swipeService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessSwipe([FromBody] SwipeRequest request)
    {
        var userId = GetCurrentUserId();

        try
        {
            var result = await _swipeService.ProcessSwipeAsync(userId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Error = ex.Message,
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }
}