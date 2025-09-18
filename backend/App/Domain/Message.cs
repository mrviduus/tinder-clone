namespace App.Domain;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MatchId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; }
    public Guid? ImagePhotoId { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public Match Match { get; set; } = null!;
    public User Sender { get; set; } = null!;
    public Photo? ImagePhoto { get; set; }
}