using ProposalGenerator.Web.Models.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProposalGenerator.Web.Services;

public class DocumentService : IDocumentService
{
    private readonly IBlobStorageService _blobStorage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DocumentService> _logger;
    private readonly IWebHostEnvironment _env;

    // Vodafone brand colours
    private static readonly string VodafoneRed = "#E60000";
    private static readonly string DarkGrey = "#333333";
    private static readonly string LightGrey = "#F4F4F4";
    private static readonly string MediumGrey = "#666666";
    private static readonly string TableHeaderBg = "#E60000";
    private static readonly string TableAltRowBg = "#FFF5F5";

    public DocumentService(
        IBlobStorageService blobStorage,
        IConfiguration configuration,
        ILogger<DocumentService> logger,
        IWebHostEnvironment env)
    {
        _blobStorage = blobStorage;
        _configuration = configuration;
        _logger = logger;
        _env = env;
    }

    public Task<byte[]> GeneratePreviewPdfAsync(Quote quote)
    {
        return Task.FromResult(GeneratePdf(quote, isPreview: true));
    }

    public Task<byte[]> GenerateFinalPdfAsync(Quote quote)
    {
        return Task.FromResult(GeneratePdf(quote, isPreview: false));
    }

    public Task<List<string>> GetAvailableTemplatesAsync()
    {
        // With QuestPDF we use code-based templates instead of .docx files
        var templates = new List<string>
        {
            "vodafone-business",
            "vodafone-compact"
        };
        return Task.FromResult(templates);
    }

    // --- Private helpers -------------------------------------------------------

    private byte[] GeneratePdf(Quote quote, bool isPreview)
    {
        var logoBytes = LoadLogoBytes();

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(30);
                page.MarginHorizontal(40);

                page.Header().Element(c => ComposeHeader(c, logoBytes, isPreview));
                page.Content().Element(c => ComposeContent(c, quote, isPreview));
                page.Footer().Element(c => ComposeFooter(c, quote));
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private byte[]? LoadLogoBytes()
    {
        try
        {
            var logoPath = Path.Combine(_env.WebRootPath, "images", "vodafone-logo.png");
            if (File.Exists(logoPath))
                return File.ReadAllBytes(logoPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load Vodafone logo for PDF");
        }
        return null;
    }

    // --- Header ----------------------------------------------------------------

    private void ComposeHeader(IContainer container, byte[]? logoBytes, bool isPreview)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                if (logoBytes != null)
                {
                    row.ConstantItem(160).Height(45).Image(logoBytes).FitArea();
                }
                else
                {
                    row.ConstantItem(160).Height(45)
                        .AlignMiddle()
                        .Text("Vodafone Business")
                        .FontSize(18).Bold().FontColor(VodafoneRed);
                }

                row.RelativeItem().AlignRight().AlignMiddle()
                    .Text(isPreview ? "QUOTE PREVIEW" : "BUSINESS PROPOSAL")
                    .FontSize(14).Bold().FontColor(DarkGrey);
            });

            // Red accent bar
            column.Item().PaddingTop(8)
                .Height(3)
                .Background(VodafoneRed);
        });
    }

    // --- Content ---------------------------------------------------------------

    private void ComposeContent(IContainer container, Quote quote, bool isPreview)
    {
        container.PaddingTop(20).Column(column =>
        {
            // Preview watermark notice
            if (isPreview)
            {
                column.Item().PaddingBottom(10)
                    .Background("#FFF3CD").Padding(8)
                    .Text("PREVIEW - This document has not been finalised.")
                    .FontSize(10).FontColor("#856404");
            }

            // Quote Details
            column.Item().PaddingBottom(15).Element(c => ComposeQuoteDetails(c, quote));

            // Customer Information
            column.Item().PaddingBottom(15).Element(c => ComposeCustomerInfo(c, quote));

            // Pricing Table
            if (quote.LineItems.Any())
            {
                column.Item().PaddingBottom(15).Element(c => ComposePricingTable(c, quote));
            }

            // Total
            column.Item().PaddingBottom(15).Element(c => ComposeTotal(c, quote));

            // Notes
            if (!string.IsNullOrEmpty(quote.Notes))
            {
                column.Item().Element(c => ComposeNotes(c, quote));
            }
        });
    }

    private void ComposeQuoteDetails(IContainer container, Quote quote)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(6)
                .Text("Quote Details").FontSize(14).Bold().FontColor(VodafoneRed);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(140);
                    cols.RelativeColumn();
                });

                AddDetailRow(table, "Quote Number", quote.QuoteNumber);
                AddDetailRow(table, "Date", quote.CreatedAt.ToString("dd MMMM yyyy"));
                AddDetailRow(table, "Valid Until", quote.ValidUntil.ToString("dd MMMM yyyy"));
                AddDetailRow(table, "Currency", quote.Currency);
            });
        });
    }

    private void ComposeCustomerInfo(IContainer container, Quote quote)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(6)
                .Text("Customer Information").FontSize(14).Bold().FontColor(VodafoneRed);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(140);
                    cols.RelativeColumn();
                });

                AddDetailRow(table, "Name", quote.CustomerName);
                if (!string.IsNullOrEmpty(quote.CustomerCompany))
                    AddDetailRow(table, "Company", quote.CustomerCompany);
                if (!string.IsNullOrEmpty(quote.CustomerEmail))
                    AddDetailRow(table, "Email", quote.CustomerEmail);
            });
        });
    }

    private static void AddDetailRow(TableDescriptor table, string label, string value)
    {
        table.Cell().PaddingVertical(3).Text(label).FontSize(10).Bold().FontColor("#333333");
        table.Cell().PaddingVertical(3).Text(value).FontSize(10).FontColor("#333333");
    }

    // --- Pricing table ---------------------------------------------------------

    private void ComposePricingTable(IContainer container, Quote quote)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(6)
                .Text("Pricing").FontSize(14).Bold().FontColor(VodafoneRed);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);    // Product
                    cols.RelativeColumn(1.5f); // SKU
                    cols.ConstantColumn(40);   // Qty
                    cols.RelativeColumn(1.2f); // Term
                    cols.RelativeColumn(1.2f); // Billing
                    cols.RelativeColumn(1.5f); // Unit Price
                    cols.ConstantColumn(55);   // Discount
                    cols.RelativeColumn(1.5f); // Total
                });

                // Header
                var headers = new[] { "Product", "SKU", "Qty", "Term", "Billing", "Unit Price", "Disc.", "Line Total" };
                foreach (var h in headers)
                {
                    table.Cell()
                        .Background(TableHeaderBg)
                        .Padding(5)
                        .Text(h).FontSize(8).Bold().FontColor(Colors.White);
                }

                // Data rows
                var rowIndex = 0;
                foreach (var item in quote.LineItems)
                {
                    var bg = rowIndex % 2 == 1 ? TableAltRowBg : "#FFFFFF";

                    PriceCell(table, item.ProductName, bg);
                    PriceCell(table, item.Sku, bg);
                    PriceCell(table, item.Quantity.ToString(), bg);
                    PriceCell(table, item.CommitmentTerm ?? "-", bg);
                    PriceCell(table, item.BillingFrequency ?? "-", bg);
                    PriceCell(table, $"{item.UnitPrice:N2} {item.Currency}", bg);
                    PriceCell(table, item.DiscountPercent.HasValue ? $"{item.DiscountPercent:N1}%" : "-", bg);
                    PriceCell(table, $"{item.LineTotal:N2} {item.Currency}", bg);

                    rowIndex++;
                }
            });
        });
    }

    private static void PriceCell(TableDescriptor table, string text, string bg)
    {
        table.Cell()
            .Background(bg)
            .BorderBottom(1).BorderColor("#E0E0E0")
            .Padding(4)
            .Text(text).FontSize(8).FontColor("#333333");
    }

    // --- Total -----------------------------------------------------------------

    private void ComposeTotal(IContainer container, Quote quote)
    {
        container.AlignRight().Row(row =>
        {
            row.AutoItem()
                .Background(LightGrey)
                .Border(1).BorderColor("#E0E0E0")
                .Padding(12)
                .Text(text =>
                {
                    text.Span("Total: ").FontSize(14).Bold().FontColor(DarkGrey);
                    text.Span($"{quote.TotalAmount:N2} {quote.Currency}")
                        .FontSize(14).Bold().FontColor(VodafoneRed);
                });
        });
    }

    // --- Notes -----------------------------------------------------------------

    private void ComposeNotes(IContainer container, Quote quote)
    {
        container.Column(col =>
        {
            col.Item().PaddingBottom(6)
                .Text("Notes").FontSize(14).Bold().FontColor(VodafoneRed);
            col.Item()
                .Background(LightGrey)
                .Padding(10)
                .Text(quote.Notes ?? "").FontSize(10).FontColor(DarkGrey);
        });
    }

    // --- Footer ----------------------------------------------------------------

    private void ComposeFooter(IContainer container, Quote quote)
    {
        container.Column(col =>
        {
            col.Item().Height(2).Background(VodafoneRed);

            col.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem()
                    .Text($"Vodafone Business  |  {quote.QuoteNumber}")
                    .FontSize(7).FontColor(MediumGrey);

                row.RelativeItem().AlignRight()
                    .Text(text =>
                    {
                        text.Span("Page ").FontSize(7).FontColor(MediumGrey);
                        text.CurrentPageNumber().FontSize(7).FontColor(MediumGrey);
                        text.Span(" of ").FontSize(7).FontColor(MediumGrey);
                        text.TotalPages().FontSize(7).FontColor(MediumGrey);
                    });
            });

            col.Item().PaddingTop(2)
                .Text("This document is confidential and intended for the named recipient only.")
                .FontSize(6).FontColor("#999999");
        });
    }
}
