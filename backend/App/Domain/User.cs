using Microsoft.AspNetCore.Identity;

namespace App.Domain;

public class User : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Profile? Profile { get; set; }
    public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    public ICollection<Swipe> SwipesMade { get; set; } = new List<Swipe>();
    public ICollection<Swipe> SwipesReceived { get; set; } = new List<Swipe>();
    public ICollection<Block> BlocksMade { get; set; } = new List<Block>();
    public ICollection<Block> BlocksReceived { get; set; } = new List<Block>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}