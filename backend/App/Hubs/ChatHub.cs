using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using App.DTOs;
using App.Services;

namespace App.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly MatchService _matchService;
    private readonly MessageService _messageService;

    public ChatHub(MatchService matchService, MessageService messageService)
    {
        _matchService = matchService;
        _messageService = messageService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    public async Task JoinMatch(string matchId)
    {
        if (!Guid.TryParse(matchId, out var matchGuid))
            return;

        var userId = GetCurrentUserId();

        // Verify user is part of the match
        if (await _matchService.IsUserInMatchAsync(matchGuid, userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"match_{matchId}");
        }
    }

    public async Task SendMessage(SendMessageRequest request)
    {
        var userId = GetCurrentUserId();

        // Send message through service
        var message = await _messageService.SendMessageAsync(userId, request);

        if (message != null)
        {
            // Broadcast to match group
            await Clients.Group($"match_{request.MatchId}")
                .SendAsync("MessageReceived", message);
        }
    }

    public async Task Typing(TypingRequest request)
    {
        var userId = GetCurrentUserId();

        // Verify user is part of the match
        if (await _matchService.IsUserInMatchAsync(request.MatchId, userId))
        {
            // Broadcast typing indicator to others in the match
            await Clients.OthersInGroup($"match_{request.MatchId}")
                .SendAsync("Typing", new
                {
                    MatchId = request.MatchId,
                    UserId = userId,
                    IsTyping = request.IsTyping
                });
        }
    }

    public async Task MarkRead(MarkReadRequest request)
    {
        var userId = GetCurrentUserId();

        var success = await _messageService.MarkMessagesAsReadAsync(userId, request);

        if (success)
        {
            // Broadcast read receipt to match group
            await Clients.Group($"match_{request.MatchId}")
                .SendAsync("ReadReceipt", new
                {
                    MatchId = request.MatchId,
                    MessageIds = request.MessageIds,
                    At = DateTime.UtcNow
                });
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any typing indicators when user disconnects
        // This is automatically handled by SignalR groups
        await base.OnDisconnectedAsync(exception);
    }
}