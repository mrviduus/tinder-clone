namespace App.Domain;

public class Swipe
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SwiperId { get; set; }
    public Guid TargetId { get; set; }
    public SwipeDirection Direction { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Swiper { get; set; } = null!;
    public User Target { get; set; } = null!;
}