using System.ComponentModel.DataAnnotations;
using App.Domain;

namespace App.DTOs;

public class ProfileResponse
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string? Bio { get; set; }
    public Gender SearchGender { get; set; }
    public int AgeMin { get; set; }
    public int AgeMax { get; set; }
    public int MaxDistanceKm { get; set; }
    public bool HasLocation { get; set; }
    public DateTime? LocationUpdatedAt { get; set; }
    public List<PhotoInfo> Photos { get; set; } = new();
}

public class UpdateProfileRequest
{
    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    public Gender? SearchGender { get; set; }

    [Range(18, 100)]
    public int? AgeMin { get; set; }

    [Range(18, 100)]
    public int? AgeMax { get; set; }

    [Range(1, 500)]
    public int? MaxDistanceKm { get; set; }
}

public class UpdateLocationRequest
{
    [Required]
    [Range(-90, 90)]
    public double Lat { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Lng { get; set; }
}

public class PublicProfileResponse
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public string? Bio { get; set; }
    public double? DistanceKm { get; set; }
    public List<PhotoInfo> Photos { get; set; } = new();
}

public class PhotoInfo
{
    public Guid PhotoId { get; set; }
    public bool IsPrimary { get; set; }
    public string Url { get; set; } = string.Empty;
}