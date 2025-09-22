using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
    [JsonIgnore]
    public bool Matched { get; set; }

    [JsonIgnore]
    public Guid? MatchId { get; set; }

    // Only expose the camelCase properties for JSON serialization
    [JsonPropertyName("isMatch")]
    public bool isMatch => Matched;

    [JsonPropertyName("matchId")]
    public string? matchId => MatchId?.ToString();
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