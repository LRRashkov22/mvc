using Microsoft.Extensions.Options;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Services.Implementations;

public sealed class ProductImageService : IProductImageService
{
    private readonly IProductImageRepository _images;
    private readonly IFileStorageService _files;
    private readonly FileStorageOptions _options;

    public ProductImageService(
        IProductImageRepository images,
        IFileStorageService files,
        IOptions<FileStorageOptions> options)
    {
        _images = images;
        _files = files;
        _options = options.Value;
    }

    public async Task<ProductImage> AddAsync(int productId, IFormFile file, CancellationToken ct = default)
    {
        if (!await _images.ProductExistsAsync(productId, ct))
            throw new KeyNotFoundException($"Product {productId} not found.");

        if (await _images.CountByProductAsync(productId, ct) >= _options.MaxImagesPerProduct)
            throw new InvalidOperationException(
                $"A product can have at most {_options.MaxImagesPerProduct} images.");

        var folder = FolderFor(productId);
        var stored = await _files.SaveImageAsync(file, folder, ct);

        var entity = new ProductImage
        {
            ProductId = productId,
            FileName = stored.FileName,
            ThumbFileName = stored.ThumbFileName,
            OriginalFileName = stored.OriginalFileName,
            SizeBytes = stored.SizeBytes,
            Width = stored.Width,
            Height = stored.Height,
            UploadedAtUtc = DateTime.UtcNow
        };

        try
        {
            await _images.AddAsync(entity, ct);
            await _images.SaveChangesAsync(ct);
        }
        catch
        {
            // Don't leave orphaned files if the database write fails.
            _files.Delete(folder, stored.FileName, stored.ThumbFileName);
            throw;
        }

        return entity;
    }

    public async Task<bool> DeleteAsync(int productId, int imageId, CancellationToken ct = default)
    {
        var image = await _images.GetAsync(productId, imageId, ct);
        if (image is null) return false;

        await _images.DeleteAsync(image, ct);
        await _images.SaveChangesAsync(ct);

        _files.Delete(FolderFor(productId), image.FileName, image.ThumbFileName);
        return true;
    }

    private static string FolderFor(int productId) => $"products/{productId}";
}