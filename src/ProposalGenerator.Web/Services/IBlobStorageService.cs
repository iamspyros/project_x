namespace ProposalGenerator.Web.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string containerPath, string fileName, byte[] content, string contentType);
    Task<byte[]?> DownloadAsync(string containerPath, string fileName);
    Task<List<string>> ListFilesAsync(string containerPath, string? extension = null);
    Task DeleteAsync(string containerPath, string fileName);
    Task<string> GetFilePathOrUrlAsync(string containerPath, string fileName);
}
