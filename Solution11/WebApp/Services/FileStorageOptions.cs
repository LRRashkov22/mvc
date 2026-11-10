namespace WebApplication1.Services;

/// <summary>Bound from the "FileStorage" section of appsettings.json. All values have safe defaults.</summary>
public sealed class FileStorageOptions
{
    /// <summary>Relative paths are resolved against the content root. Keep this OUTSIDE wwwroot.</summary>
    public string RootPath { get; set; } = "App_Data/uploads";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public int MaxImageDimension { get; set; } = 1600;
    public int ThumbnailSize { get; set; } = 320;

    /// <summary>Rejects "decompression bomb" images (width * height above this value).</summary>
    public long MaxPixels { get; set; } = 40_000_000;

    public int MaxImagesPerProduct { get; set; } = 10;
    public int WebpQuality { get; set; } = 80;
}