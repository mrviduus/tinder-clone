using NetTopologySuite.Geometries;

namespace App.Domain;

public class Profile
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string? Bio { get; set; }
    public Gender SearchGender { get; set; }
    public int AgeMin { get; set; } = 18;
    public int AgeMax { get; set; } = 100;
    public int MaxDistanceKm { get; set; } = 50;
    public Point? Location { get; set; }
    public DateTime? LocationUpdatedAt { get; set; }

    public User User { get; set; } = null!;
}