using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using App.DTOs;
using App.Services;

namespace App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly ProfileService _profileService;
    private readonly PhotoService _photoService;

    public MeController(ProfileService profileService, PhotoService photoService)
    {
        _profileService = profileService;
        _photoService = photoService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        var profile = await _profileService.GetProfileAsync(userId);

        if (profile == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Profile not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var success = await _profileService.UpdateProfileAsync(userId, request);

        if (!success)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Profile not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var updatedProfile = await _profileService.GetProfileAsync(userId);
        return Ok(updatedProfile);
    }

    [HttpPut("location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var userId = GetCurrentUserId();
        var success = await _profileService.UpdateLocationAsync(userId, request);

        if (!success)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Profile not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return NoContent();
    }

    [HttpPost("photos")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        var userId = GetCurrentUserId();
        var (success, error, response) = await _photoService.UploadPhotoAsync(userId, file);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = error!,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Created($"/api/users/{userId}/photos/{response!.PhotoId}", response);
    }

    [HttpDelete("photos/{photoId}")]
    public async Task<IActionResult> DeletePhoto(Guid photoId)
    {
        var userId = GetCurrentUserId();
        var success = await _photoService.DeletePhotoAsync(userId, photoId);

        if (!success)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Photo not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return NoContent();
    }
}