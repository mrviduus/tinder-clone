namespace App.Domain;

public class Block
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BlockerId { get; set; }
    public Guid TargetId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Blocker { get; set; } = null!;
    public User Target { get; set; } = null!;
}