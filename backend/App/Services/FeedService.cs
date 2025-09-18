using Microsoft.EntityFrameworkCore;
using App.Data;
using App.DTOs;

namespace App.Services;

public class FeedService
{
    private readonly AppDbContext _context;

    public FeedService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CandidateResponse>> GetCandidatesAsync(Guid userId, int radiusKm = 50, int page = 1, int pageSize = 20)
    {
        // Get current user's profile
        var myProfile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (myProfile?.Location == null)
        {
            return new List<CandidateResponse>(); // No location, return empty feed
        }

        var effectiveRadius = Math.Min(myProfile.MaxDistanceKm, radiusKm) * 1000; // Convert to meters

        // Get already swiped user IDs
        var swipedUserIds = await _context.Swipes
            .Where(s => s.SwiperId == userId)
            .Select(s => s.TargetId)
            .ToListAsync();

        // Get blocked user IDs (both directions)
        var blockedUserIds = await _context.Blocks
            .Where(b => b.BlockerId == userId || b.TargetId == userId)
            .Select(b => b.BlockerId == userId ? b.TargetId : b.BlockerId)
            .ToListAsync();

        var excludedUserIds = swipedUserIds.Concat(blockedUserIds).Concat(new[] { userId }).ToList();

        // Find candidates
        var candidates = await _context.Profiles
            .Include(p => p.User)
            .Where(p => p.Location != null &&
                       !excludedUserIds.Contains(p.UserId) &&
                       p.Location.IsWithinDistance(myProfile.Location, effectiveRadius))
            .Select(p => new
            {
                p.UserId,
                p.DisplayName,
                p.BirthDate,
                p.Gender,
                p.Location,
                p.LocationUpdatedAt
            })
            .ToListAsync();

        // Filter by age and gender preferences
        var filteredCandidates = candidates
            .Where(c =>
            {
                var age = DateTime.UtcNow.Year - c.BirthDate.Year;
                if (c.BirthDate.AddYears(age) > DateTime.UtcNow) age--;

                return age >= myProfile.AgeMin && age <= myProfile.AgeMax &&
                       (myProfile.SearchGender == Domain.Gender.Unknown || c.Gender == myProfile.SearchGender);
            })
            .OrderByDescending(c => c.LocationUpdatedAt)
            .ThenByDescending(c => c.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new List<CandidateResponse>();

        foreach (var candidate in filteredCandidates)
        {
            var age = DateTime.UtcNow.Year - candidate.BirthDate.Year;
            if (candidate.BirthDate.AddYears(age) > DateTime.UtcNow) age--;

            var distanceKm = candidate.Location!.Distance(myProfile.Location) / 1000.0;

            // Get primary photo URL
            var primaryPhotoId = await _context.Photos
                .Where(p => p.UserId == candidate.UserId && p.IsPrimary)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            string? primaryPhotoUrl = null;
            if (primaryPhotoId != Guid.Empty)
            {
                primaryPhotoUrl = $"/api/users/{candidate.UserId}/photos/{primaryPhotoId}";
            }

            result.Add(new CandidateResponse
            {
                UserId = candidate.UserId,
                DisplayName = candidate.DisplayName,
                Age = age,
                DistanceKm = Math.Round(distanceKm, 1),
                PrimaryPhotoUrl = primaryPhotoUrl
            });
        }

        return result;
    }
}