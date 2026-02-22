using Aspose.Words;
using Aspose.Words.MailMerging;
using System.Data;

namespace ProposalApi.Services;

public class AsposeDocumentService : IDocumentService
{
    private readonly IBlobStorageService _blobService;
    private readonly ILogger<AsposeDocumentService> _logger;
    private readonly IConfiguration _configuration;
    private static bool _licenseApplied;
    private static readonly object _licenseLock = new();

    public AsposeDocumentService(
        IBlobStorageService blobService,
        ILogger<AsposeDocumentService> logger,
        IConfiguration configuration)
    {
        _blobService = blobService;
        _logger = logger;
        _configuration = configuration;
        EnsureLicense();
    }

    private void EnsureLicense()
    {
        if (_licenseApplied) return;
        lock (_licenseLock)
        {
            if (_licenseApplied) return;
            var licensePath = _configuration["Aspose:LicensePath"];
            if (!string.IsNullOrWhiteSpace(licensePath) && File.Exists(licensePath))
            {
                var license = new License();
                license.SetLicense(licensePath);
                _logger.LogInformation("Aspose.Words license applied from {Path}", licensePath);
            }
            else
            {
                _logger.LogWarning("Aspose.Words running in evaluation mode. Set Aspose:LicensePath for production use.");
            }
            _licenseApplied = true;
        }
    }

    public async Task<byte[]> GeneratePdfAsync(
        string templateBlobName,
        Dictionary<string, string> mergeFields,
        IEnumerable<Dictionary<string, object>> priceRows)
    {
        _logger.LogInformation("Generating PDF from template {Template}", templateBlobName);

        // 1. Download the DOCX template from Blob Storage
        var templateBytes = await _blobService.DownloadTemplateAsync(templateBlobName);
        using var templateStream = new MemoryStream(templateBytes);

        // 2. Load into Aspose.Words
        var doc = new Document(templateStream);

        // 3. Execute simple mail merge for top-level fields (CustomerName, QuoteNumber, etc.)
        if (mergeFields.Count > 0)
        {
            var fieldNames = mergeFields.Keys.ToArray();
            var fieldValues = mergeFields.Values.Cast<object>().ToArray();
            doc.MailMerge.Execute(fieldNames, fieldValues);
        }

        // 4. Execute mail merge with regions for the pricing table (region: "PriceRows")
        if (priceRows.Any())
        {
            var dataTable = BuildPriceDataTable(priceRows);
            doc.MailMerge.ExecuteWithRegions(dataTable);
        }

        // 5. Clean up unused merge fields
        doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedRegions
                                     | MailMergeCleanupOptions.RemoveEmptyParagraphs;

        // 6. Export to PDF
        using var pdfStream = new MemoryStream();
        doc.Save(pdfStream, SaveFormat.Pdf);

        _logger.LogInformation("PDF generated successfully ({Bytes} bytes)", pdfStream.Length);
        return pdfStream.ToArray();
    }

    private static DataTable BuildPriceDataTable(IEnumerable<Dictionary<string, object>> rows)
    {
        // The DataTable name must match the mail merge region name in the Word template
        var table = new DataTable("PriceRows");

        var rowList = rows.ToList();
        if (rowList.Count == 0) return table;

        // Add columns from the first row's keys
        foreach (var key in rowList[0].Keys)
        {
            table.Columns.Add(key, rowList[0][key]?.GetType() ?? typeof(string));
        }

        // Add data rows
        foreach (var row in rowList)
        {
            var dataRow = table.NewRow();
            foreach (var kvp in row)
            {
                dataRow[kvp.Key] = kvp.Value ?? DBNull.Value;
            }
            table.Rows.Add(dataRow);
        }

        return table;
    }
}
