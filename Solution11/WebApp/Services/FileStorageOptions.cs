namespace WebApplication1.Services;


public sealed class FileStorageOptions
{
    
    public string RootPath { get; set; } = "App_Data/uploads";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public int MaxImageDimension { get; set; } = 1600;
    public int ThumbnailSize { get; set; } = 320;
    
    public long MaxPixels { get; set; } = 40_000_000;

    public int MaxImagesPerProduct { get; set; } = 10;
    public int WebpQuality { get; set; } = 80;
}