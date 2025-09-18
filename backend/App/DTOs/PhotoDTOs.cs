namespace App.DTOs;

public class PhotoResponse
{
    public Guid PhotoId { get; set; }
    public bool IsPrimary { get; set; }
}

public class CandidateResponse
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Age { get; set; }
    public double DistanceKm { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
}