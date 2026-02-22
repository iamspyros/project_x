using Azure.Storage.Blobs;

namespace ProposalGenerator.Web.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly bool _useLocalFileSystem;
    private readonly string _localBasePath;

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _useLocalFileSystem = configuration.GetValue<bool>("BlobStorage:UseLocalFileSystem", true);
        _localBasePath = configuration["BlobStorage:LocalBasePath"] ?? "./storage";
    }

    public async Task<string> UploadAsync(string containerPath, string fileName, byte[] content, string contentType)
    {
        if (_useLocalFileSystem)
        {
            var dirPath = Path.Combine(_localBasePath, containerPath);
            Directory.CreateDirectory(dirPath);
            var filePath = Path.Combine(dirPath, fileName);
            await File.WriteAllBytesAsync(filePath, content);
            _logger.LogInformation("File saved locally: {Path}", filePath);
            return filePath;
        }

        var connectionString = _configuration["BlobStorage:ConnectionString"];
        var containerName = _configuration["BlobStorage:ContainerName"] ?? "proposal-documents";

        var client = new BlobServiceClient(connectionString);
        var container = client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync();

        var blobName = $"{containerPath}/{fileName}";
        var blobClient = container.GetBlobClient(blobName);

        using var stream = new MemoryStream(content);
        await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
        {
            ContentType = contentType
        });

        _logger.LogInformation("File uploaded to blob: {BlobName}", blobName);
        return blobClient.Uri.ToString();
    }

    public async Task<byte[]?> DownloadAsync(string containerPath, string fileName)
    {
        if (_useLocalFileSystem)
        {
            var filePath = Path.Combine(_localBasePath, containerPath, fileName);
            if (!File.Exists(filePath)) return null;
            return await File.ReadAllBytesAsync(filePath);
        }

        var connectionString = _configuration["BlobStorage:ConnectionString"];
        var containerName = _configuration["BlobStorage:ContainerName"] ?? "proposal-documents";

        var client = new BlobServiceClient(connectionString);
        var container = client.GetBlobContainerClient(containerName);
        var blobClient = container.GetBlobClient($"{containerPath}/{fileName}");

        if (!await blobClient.ExistsAsync()) return null;

        using var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream);
        return stream.ToArray();
    }

    public async Task<List<string>> ListFilesAsync(string containerPath, string? extension = null)
    {
        var files = new List<string>();

        if (_useLocalFileSystem)
        {
            var dirPath = Path.Combine(_localBasePath, containerPath);
            if (!Directory.Exists(dirPath)) return files;

            var pattern = string.IsNullOrEmpty(extension) ? "*" : $"*{extension}";
            files.AddRange(Directory.GetFiles(dirPath, pattern).Select(Path.GetFileName)!);
            return files;
        }

        var connectionString = _configuration["BlobStorage:ConnectionString"];
        var containerName = _configuration["BlobStorage:ContainerName"] ?? "proposal-documents";

        var client = new BlobServiceClient(connectionString);
        var container = client.GetBlobContainerClient(containerName);

        if (!await container.ExistsAsync()) return files;

        await foreach (var blob in container.GetBlobsAsync(prefix: containerPath))
        {
            var name = Path.GetFileName(blob.Name);
            if (string.IsNullOrEmpty(extension) || name.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                files.Add(name);
        }

        return files;
    }

    public async Task DeleteAsync(string containerPath, string fileName)
    {
        if (_useLocalFileSystem)
        {
            var filePath = Path.Combine(_localBasePath, containerPath, fileName);
            if (File.Exists(filePath)) File.Delete(filePath);
            return;
        }

        var connectionString = _configuration["BlobStorage:ConnectionString"];
        var containerName = _configuration["BlobStorage:ContainerName"] ?? "proposal-documents";

        var client = new BlobServiceClient(connectionString);
        var container = client.GetBlobContainerClient(containerName);
        await container.GetBlobClient($"{containerPath}/{fileName}").DeleteIfExistsAsync();
    }

    public Task<string> GetFilePathOrUrlAsync(string containerPath, string fileName)
    {
        if (_useLocalFileSystem)
        {
            return Task.FromResult(Path.Combine(_localBasePath, containerPath, fileName));
        }

        var connectionString = _configuration["BlobStorage:ConnectionString"];
        var containerName = _configuration["BlobStorage:ContainerName"] ?? "proposal-documents";
        var client = new BlobServiceClient(connectionString);
        var container = client.GetBlobContainerClient(containerName);
        return Task.FromResult(container.GetBlobClient($"{containerPath}/{fileName}").Uri.ToString());
    }
}
