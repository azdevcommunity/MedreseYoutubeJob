namespace YoutubeApiSynchronize.Core.Interfaces.File;

public interface IFileService
{
    Task<string> UploadFileAsync(string base64File, string folder);
    Task DeleteFileAsync(string fileName, string folder);
    Task DeleteFilesAsync(List<string> fileNames, string folder);
    bool IsBase64(string base64);
    string GenerateFileName();
}
