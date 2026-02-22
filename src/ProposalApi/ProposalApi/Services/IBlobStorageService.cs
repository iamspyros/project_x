namespace ProposalApi.Services;

public interface IBlobStorageService
{
    Task<byte[]> DownloadTemplateAsync(string blobName);
    Task<string> UploadPdfAsync(string blobName, byte[] pdfBytes);
    Task<Uri> GetPdfDownloadUrlAsync(string blobName, TimeSpan validFor);
    Task<IReadOnlyList<string>> ListPriceImportFilesAsync();
    Task<Stream> OpenPriceImportFileAsync(string blobName);
    Task MovePriceImportFileAsync(string blobName, string destinationFolder);
}
