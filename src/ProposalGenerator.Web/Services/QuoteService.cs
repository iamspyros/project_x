using Microsoft.EntityFrameworkCore;
using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Models.Domain;
using ProposalGenerator.Web.Models.ViewModels;

namespace ProposalGenerator.Web.Services;

public class QuoteService : IQuoteService
{
    private readonly AppDbContext _db;
    private readonly IDocumentService _documentService;
    private readonly IBlobStorageService _blobStorage;
    private readonly IAuditService _audit;
    private readonly ILogger<QuoteService> _logger;

    public QuoteService(
        AppDbContext db,
        IDocumentService documentService,
        IBlobStorageService blobStorage,
        IAuditService audit,
        ILogger<QuoteService> logger)
    {
        _db = db;
        _documentService = documentService;
        _blobStorage = blobStorage;
        _audit = audit;
        _logger = logger;
    }

    public async Task<Quote> CreateQuoteAsync(CreateQuoteViewModel model, string userId)
    {
        var quote = new Quote
        {
            QuoteNumber = GenerateQuoteNumber(),
            CustomerName = model.CustomerName,
            CustomerEmail = model.CustomerEmail,
            CustomerCompany = model.CustomerCompany,
            ValidUntil = model.ValidUntil,
            Currency = model.Currency,
            Notes = model.Notes,
            TemplateName = model.TemplateName,
            Status = QuoteStatus.Draft,
            CreatedBy = userId
        };

        // Build line items from selected products
        foreach (var itemInput in model.LineItems.Where(li => li.Quantity > 0))
        {
            var product = await _db.Products.FindAsync(itemInput.ProductId);
            if (product == null) continue;

            var discount = itemInput.DiscountPercent ?? 0;
            var unitPriceAfterDiscount = product.UnitPrice * (1 - discount / 100m);
            var lineTotal = unitPriceAfterDiscount * itemInput.Quantity;

            quote.LineItems.Add(new QuoteLineItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Sku = product.Sku,
                Quantity = itemInput.Quantity,
                CommitmentTerm = product.CommitmentTerm,
                BillingFrequency = product.BillingFrequency,
                UnitPrice = product.UnitPrice,
                DiscountPercent = itemInput.DiscountPercent,
                LineTotal = lineTotal,
                Currency = model.Currency
            });
        }

        quote.TotalAmount = quote.LineItems.Sum(li => li.LineTotal);

        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync();

        await _audit.LogAsync("QuoteCreated", "Quote", quote.Id, userId,
            $"Quote {quote.QuoteNumber} created for {quote.CustomerName}");

        _logger.LogInformation("Quote {QuoteNumber} created by {User}", quote.QuoteNumber, userId);
        return quote;
    }

    public async Task<Quote?> GetQuoteAsync(int id)
    {
        return await _db.Quotes
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Quote>> GetAllQuotesAsync()
    {
        return await _db.Quotes
            .Include(q => q.LineItems)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Quote> FinalizeQuoteAsync(int quoteId, string userId)
    {
        var quote = await _db.Quotes
            .Include(q => q.LineItems)
            .FirstOrDefaultAsync(q => q.Id == quoteId)
            ?? throw new InvalidOperationException($"Quote {quoteId} not found");

        // Generate final PDF
        var pdfBytes = await _documentService.GenerateFinalPdfAsync(quote);

        // Store PDF
        var pdfFileName = $"{quote.QuoteNumber}_v{quote.Version}.pdf";
        await _blobStorage.UploadAsync("generated-pdfs", pdfFileName, pdfBytes, "application/pdf");

        quote.Status = QuoteStatus.Finalized;
        quote.GeneratedPdfPath = pdfFileName;
        quote.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _audit.LogAsync("QuoteFinalized", "Quote", quote.Id, userId,
            $"Quote {quote.QuoteNumber} finalized, PDF: {pdfFileName}");

        _logger.LogInformation("Quote {QuoteNumber} finalized by {User}", quote.QuoteNumber, userId);
        return quote;
    }

    public async Task DeleteQuoteAsync(int quoteId)
    {
        var quote = await _db.Quotes.FindAsync(quoteId);
        if (quote != null)
        {
            _db.Quotes.Remove(quote);
            await _db.SaveChangesAsync();
        }
    }

    public string GenerateQuoteNumber()
    {
        return $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}
