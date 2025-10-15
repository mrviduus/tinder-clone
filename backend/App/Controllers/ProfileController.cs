using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using App.DTOs;
using App.Services;

namespace App.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ProfileService _profileService;
    private readonly PhotoService _photoService;

    public ProfileController(ProfileService profileService, PhotoService photoService)
    {
        _profileService = profileService;
        _photoService = photoService;
    }

    // GET /api/profile
    [HttpGet]
    public async Task<IActionResult> GetProfile()
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

        var profile = await _profileService.GetProfileAsync(userId.Value);

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

    // GET /api/profile/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPublicProfile(Guid id)
    {
        var viewerUserId = GetUserId();
        var profile = await _profileService.GetPublicProfileAsync(id, viewerUserId);

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

    // PUT /api/profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
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

        var success = await _profileService.UpdateProfileAsync(userId.Value, request);

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

    // PUT /api/profile/location
    [HttpPut("location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
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

        var success = await _profileService.UpdateLocationAsync(userId.Value, request);

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

    // POST /api/profile/photos
    [HttpPost("photos")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
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

        if (file == null || file.Length == 0)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "No file provided",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        var (success, error, response) = await _photoService.UploadPhotoAsync(userId.Value, file);

        if (!success)
        {
            return BadRequest(new ErrorResponse
            {
                Error = error!,
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return CreatedAtAction(nameof(GetPhoto), new { id = response!.PhotoId }, response);
    }

    // GET /api/profile/photos/{id}
    [HttpGet("photos/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPhoto(Guid id)
    {
        var (data, contentType) = await _photoService.GetPhotoAsync(id);

        if (data == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Photo not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return File(data, contentType!);
    }

    // DELETE /api/profile/photos/{id}
    [HttpDelete("photos/{id:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid id)
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

        var success = await _photoService.DeletePhotoAsync(userId.Value, id);

        if (!success)
        {
            return NotFound(new ErrorResponse
            {
                Error = "Photo not found or you don't have permission to delete it",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return NoContent();
    }

    // PUT /api/profile/photos/{id}/primary
    [HttpPut("photos/{id:guid}/primary")]
    public async Task<IActionResult> SetPrimaryPhoto(Guid id)
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

        // This method needs to be added to PhotoService
        return StatusCode(501, new ErrorResponse
        {
            Error = "Set primary photo not yet implemented",
            TraceId = HttpContext.TraceIdentifier
        });
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