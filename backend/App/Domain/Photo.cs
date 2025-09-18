namespace App.Domain;

public class Photo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int SizeBytes { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsPrimary { get; set; }

    public User User { get; set; } = null!;
}