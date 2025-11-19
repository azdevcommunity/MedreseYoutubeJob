using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using YoutubeApiSynchronize.Core.Interfaces.File;
using YoutubeApiSynchronize.Core.Options;
using ILogger = Serilog.ILogger;

namespace YoutubeApiSynchronize.Application.Services.File;

public class FileService : IFileService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger _logger;

    public FileService(IOptions<CloudinarySettings> cloudinarySettings, ILogger logger)
    {
        _logger = logger;
        
        var account = new Account(
            cloudinarySettings.Value.CloudName,
            cloudinarySettings.Value.ApiKey,
            cloudinarySettings.Value.ApiSecret
        );
        
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadFileAsync(string base64File, string folder)
    {
        if (string.IsNullOrEmpty(base64File))
        {
            throw new ArgumentException("base64File cannot be null or empty");
        }

        try
        {
            var fileName = GenerateFileName();
            
            // Remove Base64 prefix if present (e.g., "data:image/png;base64,")
            if (base64File.Contains(","))
            {
                base64File = base64File.Split(',')[1];
            }

            // Decode Base64 string to byte array
            var decodedBytes = Convert.FromBase64String(base64File);

            using var stream = new MemoryStream(decodedBytes);
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                PublicId = fileName,
                AssetFolder = folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.Error("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                throw new Exception($"File upload failed: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error while uploading file to Cloudinary");
            throw new Exception("File upload failed", ex);
        }
    }

    public async Task DeleteFileAsync(string fileName, string folder)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        await DeleteFilesAsync(new List<string> { fileName }, folder);
    }

    public async Task DeleteFilesAsync(List<string> fileNames, string folder)
    {
        try
        {
            if (fileNames == null || !fileNames.Any())
            {
                return;
            }

            var publicIds = fileNames.Select(path =>
            {
                var name = path.Substring(path.LastIndexOf('/') + 1);
                var dotIndex = name.LastIndexOf('.');
                return dotIndex == -1 ? name : name.Substring(0, dotIndex);
            }).ToList();

            var deleteParams = new DelResParams
            {
                PublicIds = publicIds,
                Type = "upload",
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DeleteResourcesAsync(deleteParams);
            
            _logger.Information("Cloudinary delete result: {Result}", result.JsonObj);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error while deleting files from Cloudinary");
            // Don't throw exception for delete operations
        }
    }

    public bool IsBase64(string base64)
    {
        if (string.IsNullOrEmpty(base64))
        {
            return false;
        }

        try
        {
            // Remove Base64 prefix if present
            if (base64.Contains(","))
            {
                base64 = base64.Split(',')[1];
            }

            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateFileName()
    {
        var guid = Guid.NewGuid().ToString("N"); // Without dashes
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        return $"{guid}_{timestamp}";
    }
}
