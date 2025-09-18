using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using App.DTOs;
using App.Services;

namespace App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ProfileService _profileService;
    private readonly PhotoService _photoService;

    public UsersController(ProfileService profileService, PhotoService photoService)
    {
        _profileService = profileService;
        _photoService = photoService;
    }

    private Guid? GetCurrentUserId()
    {
        if (!User.Identity?.IsAuthenticated == true) return null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetPublicProfile(Guid id)
    {
        var viewerUserId = GetCurrentUserId();
        var profile = await _profileService.GetPublicProfileAsync(id, viewerUserId);

        if (profile == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "User not found",
                TraceId = HttpContext.TraceIdentifier
            });
        }

        return Ok(profile);
    }

    [HttpGet("{userId}/photos/{photoId}")]
    public async Task<IActionResult> GetPhoto(Guid userId, Guid photoId)
    {
        var (data, contentType) = await _photoService.GetPhotoAsync(photoId);

        if (data == null || contentType == null)
        {
            return NotFound();
        }

        return File(data, contentType, enableRangeProcessing: true);
    }
}