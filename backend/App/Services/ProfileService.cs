using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using App.Data;
using App.DTOs;

namespace App.Services;

public class ProfileService
{
    private readonly AppDbContext _context;

    public ProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileResponse?> GetProfileAsync(Guid userId)
    {
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return null;

        var age = DateTime.UtcNow.Year - profile.BirthDate.Year;
        if (profile.BirthDate.AddYears(age) > DateTime.UtcNow) age--;

        return new ProfileResponse
        {
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            BirthDate = profile.BirthDate,
            Age = age,
            Gender = profile.Gender,
            Bio = profile.Bio,
            SearchGender = profile.SearchGender,
            AgeMin = profile.AgeMin,
            AgeMax = profile.AgeMax,
            MaxDistanceKm = profile.MaxDistanceKm,
            HasLocation = profile.Location != null,
            LocationUpdatedAt = profile.LocationUpdatedAt
        };
    }

    public async Task<PublicProfileResponse?> GetPublicProfileAsync(Guid userId, Guid? viewerUserId = null)
    {
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return null;

        var age = DateTime.UtcNow.Year - profile.BirthDate.Year;
        if (profile.BirthDate.AddYears(age) > DateTime.UtcNow) age--;

        double? distanceKm = null;
        if (viewerUserId.HasValue && profile.Location != null)
        {
            var viewerProfile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == viewerUserId.Value);

            if (viewerProfile?.Location != null)
            {
                distanceKm = profile.Location.Distance(viewerProfile.Location) / 1000.0;
            }
        }

        return new PublicProfileResponse
        {
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            Age = age,
            Gender = profile.Gender,
            Bio = profile.Bio,
            DistanceKm = distanceKm
        };
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return false;

        if (request.DisplayName != null)
            profile.DisplayName = request.DisplayName;

        if (request.Bio != null)
            profile.Bio = request.Bio;

        if (request.SearchGender.HasValue)
            profile.SearchGender = request.SearchGender.Value;

        if (request.AgeMin.HasValue)
            profile.AgeMin = request.AgeMin.Value;

        if (request.AgeMax.HasValue)
            profile.AgeMax = request.AgeMax.Value;

        if (request.MaxDistanceKm.HasValue)
            profile.MaxDistanceKm = request.MaxDistanceKm.Value;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLocationAsync(Guid userId, UpdateLocationRequest request)
    {
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return false;

        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        profile.Location = geometryFactory.CreatePoint(new Coordinate(request.Lng, request.Lat));
        profile.LocationUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}