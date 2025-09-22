using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace App.DTOs;

public class MessageResponse
{
    [JsonPropertyName("messageId")]
    public Guid Id { get; set; }

    [JsonPropertyName("matchId")]
    public Guid MatchId { get; set; }

    [JsonPropertyName("senderId")]
    public Guid SenderId { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("imagePhotoId")]
    public Guid? ImagePhotoId { get; set; }

    [JsonPropertyName("sentAt")]
    public DateTime SentAt { get; set; }

    [JsonPropertyName("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    [JsonPropertyName("readAt")]
    public DateTime? ReadAt { get; set; }
}

public class SendMessageRequest
{
    [Required]
    public Guid MatchId { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("photoId")]
    public Guid? PhotoId { get; set; }
}

public class MarkReadRequest
{
    [Required]
    public Guid MatchId { get; set; }

    [Required]
    public List<Guid> MessageIds { get; set; } = new();
}

public class TypingRequest
{
    [Required]
    public Guid MatchId { get; set; }

    [Required]
    public bool IsTyping { get; set; }
}