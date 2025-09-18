using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Domain;
using App.DTOs;

namespace App.Services;

public class SwipeService
{
    private readonly AppDbContext _context;

    public SwipeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SwipeResponse> ProcessSwipeAsync(Guid userId, SwipeRequest request)
    {
        var direction = request.Direction.ToLower() switch
        {
            "like" => SwipeDirection.Like,
            "pass" => SwipeDirection.Pass,
            _ => throw new ArgumentException("Invalid swipe direction")
        };

        // Check if swipe already exists (idempotency)
        var existingSwipe = await _context.Swipes
            .FirstOrDefaultAsync(s => s.SwiperId == userId && s.TargetId == request.TargetUserId);

        if (existingSwipe != null)
        {
            // Check if there's already a match
            var existingMatch = await _context.Matches
                .FirstOrDefaultAsync(m =>
                    (m.UserAId == userId && m.UserBId == request.TargetUserId) ||
                    (m.UserAId == request.TargetUserId && m.UserBId == userId));

            return new SwipeResponse
            {
                Matched = existingMatch != null,
                MatchId = existingMatch?.Id
            };
        }

        // Create new swipe
        var swipe = new Swipe
        {
            SwiperId = userId,
            TargetId = request.TargetUserId,
            Direction = direction
        };

        _context.Swipes.Add(swipe);

        // Check for mutual like and create match
        Guid? matchId = null;
        bool matched = false;

        if (direction == SwipeDirection.Like)
        {
            var mutualSwipe = await _context.Swipes
                .FirstOrDefaultAsync(s =>
                    s.SwiperId == request.TargetUserId &&
                    s.TargetId == userId &&
                    s.Direction == SwipeDirection.Like);

            if (mutualSwipe != null)
            {
                // Create match with consistent ordering (smaller ID first)
                var userAId = userId < request.TargetUserId ? userId : request.TargetUserId;
                var userBId = userId < request.TargetUserId ? request.TargetUserId : userId;

                var match = new Match
                {
                    UserAId = userAId,
                    UserBId = userBId
                };

                _context.Matches.Add(match);
                matchId = match.Id;
                matched = true;
            }
        }

        await _context.SaveChangesAsync();

        return new SwipeResponse
        {
            Matched = matched,
            MatchId = matchId
        };
    }
}