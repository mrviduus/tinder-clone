using Microsoft.EntityFrameworkCore;
using App.Data;
using App.DTOs;

namespace App.Services;

public class MatchService
{
    private readonly AppDbContext _context;
    private readonly ProfileService _profileService;

    public MatchService(AppDbContext context, ProfileService profileService)
    {
        _context = context;
        _profileService = profileService;
    }

    public async Task<List<MatchResponse>> GetMatchesAsync(Guid userId, int page = 1, int pageSize = 50)
    {
        var matches = await _context.Matches
            .Include(m => m.Messages)
            .Where(m => m.UserAId == userId || m.UserBId == userId)
            .OrderByDescending(m => m.Messages.Any() ? m.Messages.Max(msg => msg.SentAt) : m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<MatchResponse>();

        foreach (var match in matches)
        {
            var counterpartId = match.UserAId == userId ? match.UserBId : match.UserAId;
            var counterpart = await _profileService.GetPublicProfileAsync(counterpartId, userId);

            if (counterpart == null) continue;

            var lastMessage = match.Messages
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var unreadCount = match.Messages
                .Count(m => m.SenderId != userId && m.ReadAt == null);

            result.Add(new MatchResponse
            {
                MatchId = match.Id,
                Counterpart = counterpart,
                LastMessagePreview = lastMessage?.Content ?? "Say hello!",
                UnreadCount = unreadCount,
                CreatedAt = match.CreatedAt
            });
        }

        return result;
    }

    public async Task<MatchDetailsResponse?> GetMatchDetailsAsync(Guid matchId, Guid userId)
    {
        var match = await _context.Matches
            .FirstOrDefaultAsync(m => m.Id == matchId && (m.UserAId == userId || m.UserBId == userId));

        if (match == null) return null;

        var userA = await _profileService.GetPublicProfileAsync(match.UserAId);
        var userB = await _profileService.GetPublicProfileAsync(match.UserBId);

        if (userA == null || userB == null) return null;

        return new MatchDetailsResponse
        {
            MatchId = match.Id,
            UserA = userA,
            UserB = userB,
            CreatedAt = match.CreatedAt
        };
    }

    public async Task<bool> IsUserInMatchAsync(Guid matchId, Guid userId)
    {
        return await _context.Matches
            .AnyAsync(m => m.Id == matchId && (m.UserAId == userId || m.UserBId == userId));
    }

    public async Task<bool> UnmatchAsync(Guid matchId, Guid userId)
    {
        var match = await _context.Matches
            .Include(m => m.Messages)
            .FirstOrDefaultAsync(m => m.Id == matchId && (m.UserAId == userId || m.UserBId == userId));

        if (match == null) return false;

        // Delete all messages in the match
        _context.Messages.RemoveRange(match.Messages);

        // Delete the match itself
        _context.Matches.Remove(match);

        // Delete the swipes between these users
        var otherUserId = match.UserAId == userId ? match.UserBId : match.UserAId;
        var swipes = await _context.Swipes
            .Where(s => (s.SwiperId == userId && s.TargetId == otherUserId) ||
                       (s.SwiperId == otherUserId && s.TargetId == userId))
            .ToListAsync();

        _context.Swipes.RemoveRange(swipes);

        await _context.SaveChangesAsync();
        return true;
    }
}