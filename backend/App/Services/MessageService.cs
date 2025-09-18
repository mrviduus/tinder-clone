using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Domain;
using App.DTOs;

namespace App.Services;

public class MessageService
{
    private readonly AppDbContext _context;
    private readonly MatchService _matchService;

    public MessageService(AppDbContext context, MatchService matchService)
    {
        _context = context;
        _matchService = matchService;
    }

    public async Task<List<MessageResponse>> GetMessagesAsync(Guid matchId, Guid userId, DateTime? before = null, int take = 30)
    {
        // Verify user is in match
        if (!await _matchService.IsUserInMatchAsync(matchId, userId))
            return new List<MessageResponse>();

        var query = _context.Messages
            .Where(m => m.MatchId == matchId);

        if (before.HasValue)
        {
            query = query.Where(m => m.SentAt < before.Value);
        }

        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return messages.Select(m => new MessageResponse
        {
            Id = m.Id,
            MatchId = m.MatchId,
            SenderId = m.SenderId,
            Content = m.Content,
            ImagePhotoId = m.ImagePhotoId,
            SentAt = m.SentAt,
            DeliveredAt = m.DeliveredAt,
            ReadAt = m.ReadAt
        }).ToList();
    }

    public async Task<MessageResponse?> SendMessageAsync(Guid userId, SendMessageRequest request)
    {
        // Verify user is in match
        if (!await _matchService.IsUserInMatchAsync(request.MatchId, userId))
            return null;

        // Validate message content
        if (string.IsNullOrEmpty(request.Text) && !request.PhotoId.HasValue)
            return null;

        // If photo is specified, verify it belongs to the sender
        if (request.PhotoId.HasValue)
        {
            var photoExists = await _context.Photos
                .AnyAsync(p => p.Id == request.PhotoId.Value && p.UserId == userId);

            if (!photoExists) return null;
        }

        var message = new Message
        {
            MatchId = request.MatchId,
            SenderId = userId,
            Content = request.Text,
            ImagePhotoId = request.PhotoId,
            DeliveredAt = DateTime.UtcNow // Immediately delivered in this simple implementation
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return new MessageResponse
        {
            Id = message.Id,
            MatchId = message.MatchId,
            SenderId = message.SenderId,
            Content = message.Content,
            ImagePhotoId = message.ImagePhotoId,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt,
            ReadAt = message.ReadAt
        };
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid userId, MarkReadRequest request)
    {
        // Verify user is in match
        if (!await _matchService.IsUserInMatchAsync(request.MatchId, userId))
            return false;

        var messages = await _context.Messages
            .Where(m => request.MessageIds.Contains(m.Id) &&
                       m.MatchId == request.MatchId &&
                       m.SenderId != userId && // Can't mark own messages as read
                       m.ReadAt == null)
            .ToListAsync();

        var readAt = DateTime.UtcNow;
        foreach (var message in messages)
        {
            message.ReadAt = readAt;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}