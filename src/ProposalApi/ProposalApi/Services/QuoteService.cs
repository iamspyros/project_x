using Microsoft.EntityFrameworkCore;
using ProposalApi.Data;
using ProposalApi.Models;
using ProposalApi.Models.Dto;

namespace ProposalApi.Services;

public class QuoteService : IQuoteService
{
    private readonly ProposalDbContext _db;
    private readonly IPricingService _pricingService;
    private readonly IDocumentService _documentService;
    private readonly IBlobStorageService _blobService;
    private readonly ILogger<QuoteService> _logger;

    private const string DefaultTemplate = "proposal-template.docx";

    public QuoteService(
        ProposalDbContext db,
        IPricingService pricingService,
        IDocumentService documentService,
        IBlobStorageService blobService,
        ILogger<QuoteService> logger)
    {
        _db = db;
        _pricingService = pricingService;
        _documentService = documentService;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<(Quote quote, byte[] pdf)> GeneratePreviewAsync(
        QuotePreviewRequest request, string userId, string userName)
    {
        var quote = await BuildQuoteAsync(request.CustomerName, request.CustomerEmail,
            request.LineItems, userId, userName);
        quote.Status = QuoteStatus.Preview;

        var pdf = await GeneratePdfFromQuoteAsync(quote);

        // Preview is not persisted to DB — only finalized quotes are saved
        return (quote, pdf);
    }

    public async Task<(Quote quote, byte[] pdf, string downloadUrl)> FinalizeAsync(
        QuoteFinalizeRequest request, string userId, string userName)
    {
        var quote = await BuildQuoteAsync(request.CustomerName, request.CustomerEmail,
            request.LineItems, userId, userName);
        quote.Status = QuoteStatus.Finalized;
        quote.ValidUntil = DateTime.UtcNow.AddDays(request.ValidityDays);
        quote.Notes = request.Notes;
        quote.FinalizedAt = DateTime.UtcNow;

        var pdf = await GeneratePdfFromQuoteAsync(quote);

        // Upload PDF to Blob Storage
        var blobName = $"{quote.QuoteNumber}/v{quote.Version}/{quote.QuoteNumber}.pdf";
        await _blobService.UploadPdfAsync(blobName, pdf);
        quote.PdfBlobPath = blobName;

        // Persist quote to database
        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync();

        // Generate a time-limited download URL
        var downloadUri = await _blobService.GetPdfDownloadUrlAsync(blobName, TimeSpan.FromHours(24));

        // Audit log
        _db.AuditLogs.Add(new AuditLog
        {
            Action = "QuoteFinalized",
            EntityType = "Quote",
            EntityId = quote.Id.ToString(),
            UserId = userId,
            UserName = userName,
            Detail = $"Quote {quote.QuoteNumber} finalized for {quote.CustomerName}. Total: {quote.TotalAmount} {quote.Currency}"
        });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Quote {QuoteNumber} finalized by {User}", quote.QuoteNumber, userName);

        return (quote, pdf, downloadUri.ToString());
    }

    public async Task<Quote?> GetQuoteAsync(int quoteId)
    {
        return await _db.Quotes
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId);
    }

    public async Task<List<Quote>> GetUserQuotesAsync(string userId)
    {
        return await _db.Quotes
            .Include(q => q.LineItems)
            .Where(q => q.CreatedByUserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    private async Task<Quote> BuildQuoteAsync(
        string customerName, string? customerEmail,
        List<QuoteLineItemRequest> lineItemRequests,
        string userId, string userName)
    {
        var productIds = lineItemRequests.Select(li => li.ProductId).Distinct();
        var products = await _pricingService.GetProductsByIdsAsync(productIds);
        var productMap = products.ToDictionary(p => p.Id);

        var quote = new Quote
        {
            QuoteNumber = GenerateQuoteNumber(),
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CreatedByUserId = userId,
            CreatedByUserName = userName,
            Currency = "EUR",
            ValidUntil = DateTime.UtcNow.AddDays(30)
        };

        foreach (var item in lineItemRequests)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
                throw new ArgumentException($"Product {item.ProductId} not found");

            var unitPrice = product.UnitPrice;
            var discountMultiplier = 1 - (item.DiscountPercent / 100m);
            var lineTotal = unitPrice * item.Quantity * discountMultiplier;

            quote.LineItems.Add(new QuoteLineItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Sku = product.Sku,
                Quantity = item.Quantity,
                CommitmentTerm = product.CommitmentTerm,
                BillingFrequency = product.BillingFrequency,
                UnitPrice = unitPrice,
                DiscountPercent = item.DiscountPercent,
                LineTotal = Math.Round(lineTotal, 2),
                Currency = product.Currency
            });
        }

        quote.TotalAmount = quote.LineItems.Sum(li => li.LineTotal);
        return quote;
    }

    private async Task<byte[]> GeneratePdfFromQuoteAsync(Quote quote)
    {
        // Top-level merge fields
        var mergeFields = new Dictionary<string, string>
        {
            ["CustomerName"] = quote.CustomerName,
            ["QuoteNumber"] = quote.QuoteNumber,
            ["QuoteDate"] = DateTime.UtcNow.ToString("dd MMMM yyyy"),
            ["ValidUntil"] = quote.ValidUntil.ToString("dd MMMM yyyy"),
            ["TotalAmount"] = quote.TotalAmount.ToString("N2"),
            ["Currency"] = quote.Currency,
            ["CreatedBy"] = quote.CreatedByUserName ?? ""
        };

        // Build pricing rows for the mail merge region
        var priceRows = quote.LineItems.Select(li => new Dictionary<string, object>
        {
            ["ProductName"] = li.ProductName,
            ["SKU"] = li.Sku,
            ["Quantity"] = li.Quantity,
            ["CommitmentTerm"] = li.CommitmentTerm ?? "",
            ["BillingFrequency"] = li.BillingFrequency ?? "",
            ["UnitPrice"] = li.UnitPrice.ToString("N2"),
            ["Discount"] = li.DiscountPercent > 0 ? $"{li.DiscountPercent}%" : "-",
            ["LineTotal"] = li.LineTotal.ToString("N2"),
            ["Currency"] = li.Currency
        }).ToList();

        return await _documentService.GeneratePdfAsync(DefaultTemplate, mergeFields, priceRows);
    }

    private static string GenerateQuoteNumber()
    {
        return $"Q-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }
}
