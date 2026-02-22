using System.Data;
using Aspose.Words;
using Aspose.Words.MailMerging;
using ProposalGenerator.Web.Models.Domain;

namespace ProposalGenerator.Web.Services;

public class DocumentService : IDocumentService
{
    private readonly IBlobStorageService _blobStorage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IBlobStorageService blobStorage,
        IConfiguration configuration,
        ILogger<DocumentService> logger)
    {
        _blobStorage = blobStorage;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePreviewPdfAsync(Quote quote)
    {
        return await GeneratePdfInternalAsync(quote, isPreview: true);
    }

    public async Task<byte[]> GenerateFinalPdfAsync(Quote quote)
    {
        return await GeneratePdfInternalAsync(quote, isPreview: false);
    }

    public async Task<List<string>> GetAvailableTemplatesAsync()
    {
        var templates = new List<string>();

        // Check local templates folder
        var templatesFolder = _configuration["Templates:FolderPath"] ?? "./templates";
        if (Directory.Exists(templatesFolder))
        {
            templates.AddRange(
                Directory.GetFiles(templatesFolder, "*.docx")
                    .Select(Path.GetFileName)!);
        }

        // Also check blob storage
        var blobTemplates = await _blobStorage.ListFilesAsync("templates", ".docx");
        templates.AddRange(blobTemplates.Where(t => !templates.Contains(t)));

        return templates;
    }

    private async Task<byte[]> GeneratePdfInternalAsync(Quote quote, bool isPreview)
    {
        var templateBytes = await LoadTemplateAsync(quote.TemplateName);
        if (templateBytes == null)
        {
            _logger.LogWarning("Template '{Template}' not found, generating basic document", quote.TemplateName);
            return GenerateBasicPdf(quote, isPreview);
        }

        using var templateStream = new MemoryStream(templateBytes);
        var doc = new Document(templateStream);

        // Simple field merge for quote-level fields
        doc.MailMerge.Execute(
            new string[]
            {
                "QuoteNumber", "CustomerName", "CustomerEmail", "CustomerCompany",
                "ValidUntil", "TotalAmount", "Currency", "Notes", "CreatedDate",
                "DocumentType"
            },
            new object[]
            {
                quote.QuoteNumber,
                quote.CustomerName,
                quote.CustomerEmail ?? "",
                quote.CustomerCompany ?? "",
                quote.ValidUntil.ToString("dd MMMM yyyy"),
                quote.TotalAmount.ToString("N2"),
                quote.Currency,
                quote.Notes ?? "",
                quote.CreatedAt.ToString("dd MMMM yyyy"),
                isPreview ? "PREVIEW" : "FINAL"
            });

        // Mail merge with regions for the pricing table
        var priceTable = BuildPriceDataTable(quote.LineItems);
        doc.MailMerge.ExecuteWithRegions(priceTable);

        // Add watermark for preview
        if (isPreview)
        {
            AddWatermark(doc, "PREVIEW");
        }

        // Export to PDF
        using var outputStream = new MemoryStream();
        doc.Save(outputStream, SaveFormat.Pdf);
        return outputStream.ToArray();
    }

    private async Task<byte[]?> LoadTemplateAsync(string? templateName)
    {
        if (string.IsNullOrEmpty(templateName))
            templateName = "default-proposal.docx";

        // Try local folder first
        var templatesFolder = _configuration["Templates:FolderPath"] ?? "./templates";
        var localPath = Path.Combine(templatesFolder, templateName);
        if (File.Exists(localPath))
        {
            return await File.ReadAllBytesAsync(localPath);
        }

        // Try blob storage
        return await _blobStorage.DownloadAsync("templates", templateName);
    }

    private DataTable BuildPriceDataTable(List<QuoteLineItem> lineItems)
    {
        var table = new DataTable("PriceRows");
        table.Columns.Add("ProductName", typeof(string));
        table.Columns.Add("Sku", typeof(string));
        table.Columns.Add("Quantity", typeof(string));
        table.Columns.Add("CommitmentTerm", typeof(string));
        table.Columns.Add("BillingFrequency", typeof(string));
        table.Columns.Add("UnitPrice", typeof(string));
        table.Columns.Add("Discount", typeof(string));
        table.Columns.Add("LineTotal", typeof(string));

        foreach (var item in lineItems)
        {
            table.Rows.Add(
                item.ProductName,
                item.Sku,
                item.Quantity.ToString(),
                item.CommitmentTerm ?? "-",
                item.BillingFrequency ?? "-",
                $"{item.UnitPrice:N2} {item.Currency}",
                item.DiscountPercent.HasValue ? $"{item.DiscountPercent:N1}%" : "-",
                $"{item.LineTotal:N2} {item.Currency}");
        }

        return table;
    }

    private void AddWatermark(Document doc, string text)
    {
        var builder = new DocumentBuilder(doc);
        var shape = new Aspose.Words.Drawing.Shape(doc, Aspose.Words.Drawing.ShapeType.TextPlainText)
        {
            TextPath = { Text = text, FontFamily = "Arial" },
            Width = 500,
            Height = 100,
            Rotation = -45,
            FillColor = System.Drawing.Color.LightGray,
            StrokeColor = System.Drawing.Color.LightGray,
            RelativeHorizontalPosition = Aspose.Words.Drawing.RelativeHorizontalPosition.Page,
            RelativeVerticalPosition = Aspose.Words.Drawing.RelativeVerticalPosition.Page,
            Left = 50,
            Top = 300,
            WrapType = Aspose.Words.Drawing.WrapType.None,
            BehindText = true
        };

        foreach (Aspose.Words.Section section in doc.Sections)
        {
            var header = section.HeadersFooters[HeaderFooterType.HeaderPrimary]
                ?? (HeaderFooter)section.HeadersFooters.Add(new HeaderFooter(doc, HeaderFooterType.HeaderPrimary));
            header.AppendChild(shape.Clone(true));
        }
    }

    private byte[] GenerateBasicPdf(Quote quote, bool isPreview)
    {
        var doc = new Document();
        var builder = new DocumentBuilder(doc);

        // Title
        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Title;
        builder.Writeln(isPreview ? "QUOTE PREVIEW" : "QUOTE");
        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;

        // Quote info
        builder.Writeln($"Quote Number: {quote.QuoteNumber}");
        builder.Writeln($"Customer: {quote.CustomerName}");
        if (!string.IsNullOrEmpty(quote.CustomerCompany))
            builder.Writeln($"Company: {quote.CustomerCompany}");
        builder.Writeln($"Date: {quote.CreatedAt:dd MMMM yyyy}");
        builder.Writeln($"Valid Until: {quote.ValidUntil:dd MMMM yyyy}");
        builder.Writeln();

        // Pricing table
        if (quote.LineItems.Any())
        {
            var table = builder.StartTable();

            // Header row
            string[] headers = { "Product", "SKU", "Qty", "Term", "Billing", "Unit Price", "Discount", "Total" };
            foreach (var header in headers)
            {
                builder.InsertCell();
                builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.FromArgb(0, 51, 102);
                builder.Font.Color = System.Drawing.Color.White;
                builder.Font.Bold = true;
                builder.Write(header);
            }
            builder.EndRow();

            // Data rows
            builder.Font.Color = System.Drawing.Color.Black;
            builder.Font.Bold = false;
            foreach (var item in quote.LineItems)
            {
                builder.InsertCell(); builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.White; builder.Write(item.ProductName);
                builder.InsertCell(); builder.Write(item.Sku);
                builder.InsertCell(); builder.Write(item.Quantity.ToString());
                builder.InsertCell(); builder.Write(item.CommitmentTerm ?? "-");
                builder.InsertCell(); builder.Write(item.BillingFrequency ?? "-");
                builder.InsertCell(); builder.Write($"{item.UnitPrice:N2} {item.Currency}");
                builder.InsertCell(); builder.Write(item.DiscountPercent.HasValue ? $"{item.DiscountPercent:N1}%" : "-");
                builder.InsertCell(); builder.Write($"{item.LineTotal:N2} {item.Currency}");
                builder.EndRow();
            }

            builder.EndTable();
            builder.Writeln();

            // Total
            builder.Font.Bold = true;
            builder.Font.Size = 14;
            builder.Writeln($"Total: {quote.TotalAmount:N2} {quote.Currency}");
        }

        if (!string.IsNullOrEmpty(quote.Notes))
        {
            builder.Font.Bold = false;
            builder.Font.Size = 10;
            builder.Writeln();
            builder.Writeln($"Notes: {quote.Notes}");
        }

        using var stream = new MemoryStream();
        doc.Save(stream, SaveFormat.Pdf);
        return stream.ToArray();
    }
}
