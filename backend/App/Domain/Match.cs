namespace App.Domain;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserAId { get; set; }
    public Guid UserBId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User UserA { get; set; } = null!;
    public User UserB { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}