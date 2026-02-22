using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace ProposalApi.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlobStorageService> _logger;

    private string TemplatesContainer => _configuration["BlobStorage:TemplatesContainer"] ?? "templates";
    private string OutputContainer => _configuration["BlobStorage:OutputContainer"] ?? "generated-pdfs";
    private string PriceImportContainer => _configuration["BlobStorage:PriceImportContainer"] ?? "price-imports";
    private string PriceImportFolder => _configuration["BlobStorage:PriceImportFolder"] ?? "incoming";

    public BlobStorageService(
        BlobServiceClient blobClient,
        IConfiguration configuration,
        ILogger<BlobStorageService> logger)
    {
        _blobClient = blobClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> DownloadTemplateAsync(string blobName)
    {
        var container = _blobClient.GetBlobContainerClient(TemplatesContainer);
        var blob = container.GetBlobClient(blobName);

        _logger.LogInformation("Downloading template {Blob} from {Container}", blobName, TemplatesContainer);

        var response = await blob.DownloadContentAsync();
        return response.Value.Content.ToArray();
    }

    public async Task<string> UploadPdfAsync(string blobName, byte[] pdfBytes)
    {
        var container = _blobClient.GetBlobContainerClient(OutputContainer);
        await container.CreateIfNotExistsAsync();

        var blob = container.GetBlobClient(blobName);
        using var stream = new MemoryStream(pdfBytes);

        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = "application/pdf" });

        _logger.LogInformation("Uploaded PDF {Blob} to {Container} ({Bytes} bytes)", blobName, OutputContainer, pdfBytes.Length);
        return blobName;
    }

    public async Task<Uri> GetPdfDownloadUrlAsync(string blobName, TimeSpan validFor)
    {
        var container = _blobClient.GetBlobContainerClient(OutputContainer);
        var blob = container.GetBlobClient(blobName);

        // Use User Delegation SAS for Entra-ID-based auth
        var userDelegationKey = await _blobClient.GetUserDelegationKeyAsync(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.Add(validFor));

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = OutputContainer,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(validFor)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = new BlobUriBuilder(blob.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, _blobClient.AccountName)
        };

        return sasUri.ToUri();
    }

    public async Task<IReadOnlyList<string>> ListPriceImportFilesAsync()
    {
        var container = _blobClient.GetBlobContainerClient(PriceImportContainer);
        var prefix = PriceImportFolder.TrimEnd('/') + "/";
        var files = new List<string>();

        await foreach (var item in container.GetBlobsAsync(prefix: prefix))
        {
            if (item.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(item.Name);
            }
        }

        return files;
    }

    public async Task<Stream> OpenPriceImportFileAsync(string blobName)
    {
        var container = _blobClient.GetBlobContainerClient(PriceImportContainer);
        var blob = container.GetBlobClient(blobName);
        var response = await blob.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task MovePriceImportFileAsync(string blobName, string destinationFolder)
    {
        var container = _blobClient.GetBlobContainerClient(PriceImportContainer);
        var sourceBlob = container.GetBlobClient(blobName);
        var fileName = Path.GetFileName(blobName);
        var destPath = $"{destinationFolder.TrimEnd('/')}/{fileName}";
        var destBlob = container.GetBlobClient(destPath);

        // Copy then delete
        await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);
        await sourceBlob.DeleteAsync();

        _logger.LogInformation("Moved price import {Source} -> {Dest}", blobName, destPath);
    }
}
