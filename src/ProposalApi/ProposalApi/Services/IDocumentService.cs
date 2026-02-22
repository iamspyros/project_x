namespace ProposalApi.Services;

public interface IDocumentService
{
    /// <summary>
    /// Generates a PDF from a Word template and pricing line items.
    /// </summary>
    Task<byte[]> GeneratePdfAsync(
        string templateBlobName,
        Dictionary<string, string> mergeFields,
        IEnumerable<Dictionary<string, object>> priceRows);
}
