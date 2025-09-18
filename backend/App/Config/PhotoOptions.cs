namespace App.Config;

public class PhotoOptions
{
    public const string SectionName = "Photos";

    public int MaxBytes { get; set; } = 5242880; // 5MB
}