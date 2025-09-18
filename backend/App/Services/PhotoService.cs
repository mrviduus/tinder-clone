using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using App.Config;
using App.Data;
using App.Domain;
using App.DTOs;

namespace App.Services;

public class PhotoService
{
    private readonly AppDbContext _context;
    private readonly PhotoOptions _photoOptions;

    public PhotoService(AppDbContext context, IOptions<PhotoOptions> photoOptions)
    {
        _context = context;
        _photoOptions = photoOptions.Value;
    }

    public async Task<(bool Success, string? Error, PhotoResponse? Response)> UploadPhotoAsync(
        Guid userId, IFormFile file)
    {
        // Validate file size
        if (file.Length > _photoOptions.MaxBytes)
        {
            return (false, $"File size exceeds maximum of {_photoOptions.MaxBytes / 1024 / 1024} MB", null);
        }

        // Validate content type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
        {
            return (false, "Only JPEG, PNG, and WebP images are allowed", null);
        }

        // Check if user exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return (false, "User not found", null);
        }

        // Check if this will be the primary photo
        var existingPhotos = await _context.Photos
            .Where(p => p.UserId == userId)
            .CountAsync();

        var isPrimary = existingPhotos == 0;

        // Read file data
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var photoData = memoryStream.ToArray();

        var photo = new Photo
        {
            UserId = userId,
            ContentType = file.ContentType,
            SizeBytes = (int)file.Length,
            Data = photoData,
            IsPrimary = isPrimary
        };

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        return (true, null, new PhotoResponse
        {
            PhotoId = photo.Id,
            IsPrimary = isPrimary
        });
    }

    public async Task<bool> DeletePhotoAsync(Guid userId, Guid photoId)
    {
        var photo = await _context.Photos
            .FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == userId);

        if (photo == null) return false;

        var wasPrimary = photo.IsPrimary;
        _context.Photos.Remove(photo);

        // If this was the primary photo, make the next one primary
        if (wasPrimary)
        {
            var nextPhoto = await _context.Photos
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.UploadedAt)
                .FirstOrDefaultAsync();

            if (nextPhoto != null)
            {
                nextPhoto.IsPrimary = true;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(byte[]? Data, string? ContentType)> GetPhotoAsync(Guid photoId)
    {
        var photo = await _context.Photos
            .FirstOrDefaultAsync(p => p.Id == photoId);

        if (photo == null) return (null, null);

        return (photo.Data, photo.ContentType);
    }

    public async Task<Guid?> GetPrimaryPhotoIdAsync(Guid userId)
    {
        var photo = await _context.Photos
            .Where(p => p.UserId == userId && p.IsPrimary)
            .FirstOrDefaultAsync();

        return photo?.Id;
    }
}