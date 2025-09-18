using App.Domain;

namespace App.DTOs;

public class PhotoResponse
{
    public Guid PhotoId { get; set; }
    public bool IsPrimary { get; set; }
}

public class FeedPhotoResponse
{
    public string PhotoId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string PhotoData { get; set; } = string.Empty; // base64 encoded
    public string UploadedAt { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}

public class FeedProfileResponse
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string? Bio { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? School { get; set; }
    public object? Location { get; set; }
    public List<FeedPhotoResponse> Photos { get; set; } = [];
    public bool? IsCurrentUser { get; set; }
}

public class CandidateResponse
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Age { get; set; }
    public double DistanceKm { get; set; }
    public string? PrimaryPhotoUrl { get; set; }
}