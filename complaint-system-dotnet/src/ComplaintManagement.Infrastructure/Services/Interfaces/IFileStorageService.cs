using Microsoft.AspNetCore.Http;

namespace ComplaintManagement.Infrastructure.Services.Interfaces;

/// <summary>
/// Service for handling file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return the stored file path/URL
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="folder">Folder to store the file in</param>
    /// <returns>File URL/path</returns>
    Task<string> UploadFileAsync(IFormFile file, string folder);

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="fileUrl">File URL/path to delete</param>
    Task DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Get file content type from file name
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <returns>Content type</returns>
    string GetContentType(string fileName);

    /// <summary>
    /// Validate if file is an image
    /// </summary>
    /// <param name="file">File to validate</param>
    /// <returns>True if file is a valid image</returns>
    bool IsValidImage(IFormFile file);
}
