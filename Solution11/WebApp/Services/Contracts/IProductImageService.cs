using WebApplication1.Models;

namespace WebApplication1.Services.Contracts;

public interface IProductImageService
{
    /// <exception cref="KeyNotFoundException">Product does not exist.</exception>
    /// <exception cref="FileValidationException">The file is not an acceptable image.</exception>
    /// <exception cref="InvalidOperationException">The product already has the maximum number of images.</exception>
    Task<ProductImage> AddAsync(int productId, IFormFile file, CancellationToken ct = default);

    /// <summary>Returns false if the image is not found for that product.</summary>
    Task<bool> DeleteAsync(int productId, int imageId, CancellationToken ct = default);
}