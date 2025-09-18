using System.ComponentModel.DataAnnotations;

namespace App.DTOs;

public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; }
    public Guid? ImagePhotoId { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class SendMessageRequest
{
    [Required]
    public Guid MatchId { get; set; }

    public string? Text { get; set; }

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