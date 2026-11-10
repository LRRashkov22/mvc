using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

// To limit uploads to admins: [Authorize(Roles = "Admin")] on Upload and Delete (use your real role name).
[Authorize]
public class ProductImagesController : Controller
{
    // Keep in sync with FileStorage:MaxFileSizeBytes: 5 files x 5 MB + overhead.
    private const int MaxFilesPerRequest = 5;
    private const int MaxRequestBytes = 26 * 1024 * 1024;

    private readonly IProductImageService _images;
    private readonly IFileStorageService _files;

    public ProductImagesController(IProductImageService images, IFileStorageService files)
    {
        _images = images;
        _files = files;
    }

    [HttpPost("products/{productId:int}/images")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxRequestBytes)]
    public async Task<IActionResult> Upload(
        int productId, List<IFormFile> files, string? returnUrl, CancellationToken ct)
    {
        if (files.Count == 0)
        {
            TempData["ImageError"] = "Please choose at least one image.";
            return BackToProduct(returnUrl);
        }

        if (files.Count > MaxFilesPerRequest)
        {
            TempData["ImageError"] = $"You can upload at most {MaxFilesPerRequest} images at once.";
            return BackToProduct(returnUrl);
        }

        var errors = new List<string>();
        var uploaded = 0;

        foreach (var file in files)
        {
            try
            {
                await _images.AddAsync(productId, file, ct);
                uploaded++;
            }
            catch (FileValidationException ex)
            {
                errors.Add($"{file.FileName}: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                errors.Add(ex.Message);
                break;
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        if (uploaded > 0) TempData["ImageSuccess"] = $"{uploaded} image(s) uploaded.";
        if (errors.Count > 0) TempData["ImageError"] = string.Join(" ", errors);

        return BackToProduct(returnUrl);
    }

    [HttpPost("products/{productId:int}/images/{imageId:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int productId, int imageId, string? returnUrl, CancellationToken ct)
    {
        var deleted = await _images.DeleteAsync(productId, imageId, ct);
        if (!deleted) return NotFound();

        TempData["ImageSuccess"] = "Image removed.";
        return BackToProduct(returnUrl);
    }

    // Images live outside wwwroot and are served only through this action.
    [HttpGet("media/products/{productId:int}/{fileName}")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
    public IActionResult Media(int productId, string fileName)
    {
        var path = _files.GetPhysicalPath($"products/{productId}", fileName);
        if (path is null) return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(path, "image/webp");
    }

    private IActionResult BackToProduct(string? returnUrl) =>
        Redirect(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
}