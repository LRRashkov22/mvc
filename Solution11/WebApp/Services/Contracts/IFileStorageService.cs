namespace WebApplication1.Services.Contracts;

public sealed record StoredImage(
    string FileName,
    string ThumbFileName,
    string OriginalFileName,
    long SizeBytes,
    int Width,
    int Height);

/// <summary>Thrown when an uploaded file fails validation. The message is safe to show to the user.</summary>
public sealed class FileValidationException : Exception
{
    public FileValidationException(string message) : base(message) { }
}

public interface IFileStorageService
{
    /// <summary>
    /// Validates, resizes, compresses (WebP) and stores an image plus a thumbnail under
    /// the given sub folder, using random file names.
    /// </summary>
    /// <exception cref="FileValidationException">The file is not an acceptable image.</exception>
    Task<StoredImage> SaveImageAsync(IFormFile file, string subFolder, CancellationToken ct = default);

    /// <summary>Deletes the named files. Names that are not server-generated are ignored.</summary>
    void Delete(string subFolder, params string[] fileNames);

    void DeleteFolder(string subFolder);

    /// <summary>Returns the full path, or null if the name is invalid or the file does not exist.</summary>
    string? GetPhysicalPath(string subFolder, string fileName);
}