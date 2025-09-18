using System.ComponentModel.DataAnnotations;
using App.Domain;

namespace App.DTOs;

public class SwipeRequest
{
    [Required]
    public Guid TargetUserId { get; set; }

    [Required]
    public string Direction { get; set; } = string.Empty; // "like" or "pass"
}

public class SwipeResponse
{
    public bool Matched { get; set; }
    public Guid? MatchId { get; set; }
}

public class MatchResponse
{
    public Guid MatchId { get; set; }
    public PublicProfileResponse Counterpart { get; set; } = null!;
    public string? LastMessagePreview { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MatchDetailsResponse
{
    public Guid MatchId { get; set; }
    public PublicProfileResponse UserA { get; set; } = null!;
    public PublicProfileResponse UserB { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}