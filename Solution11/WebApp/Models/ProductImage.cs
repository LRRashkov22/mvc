using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class ProductImage
{
    public int ProductImageId { get; set; }
    public int ProductId { get; set; }

    /// <summary>Server-generated name of the full-size image, e.g. "3f2a...c9.webp".</summary>
    [Required]
    [StringLength(64)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Server-generated name of the thumbnail, e.g. "3f2a...c9_thumb.webp".</summary>
    [Required]
    [StringLength(64)]
    public string ThumbFileName { get; set; } = string.Empty;

    /// <summary>Name the user uploaded. Display only, never used as a path.</summary>
    [StringLength(100)]
    public string OriginalFileName { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}