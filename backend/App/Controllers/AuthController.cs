using Microsoft.AspNetCore.Mvc;
using App.DTOs;
using App.Services;

namespace App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, error, response) = await _authService.RegisterAsync(request);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = error!,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return CreatedAtAction(nameof(Register), response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, error, response) = await _authService.LoginAsync(request);

        if (!success)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = error!,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var (success, error, response) = await _authService.RefreshAsync(request);

        if (!success)
        {
            return Unauthorized(new ErrorResponse
            {
                Error = error!,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}