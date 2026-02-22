using Microsoft.AspNetCore.Mvc;
using ProposalGenerator.Web.Models.ViewModels;
using ProposalGenerator.Web.Services;

namespace ProposalGenerator.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;
    private readonly IDocumentService _documentService;
    private readonly IBlobStorageService _blobStorageService;

    public QuotesController(
        IQuoteService quoteService,
        IDocumentService documentService,
        IBlobStorageService blobStorageService)
    {
        _quoteService = quoteService;
        _documentService = documentService;
        _blobStorageService = blobStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApiQuoteResponse>>> GetAll()
    {
        var quotes = await _quoteService.GetAllQuotesAsync();
        var response = quotes.Select(q => MapToResponse(q)).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiQuoteResponse>> GetById(int id)
    {
        var quote = await _quoteService.GetQuoteAsync(id);
        if (quote == null)
            return NotFound();

        return Ok(MapToResponse(quote));
    }

    [HttpPost]
    public async Task<ActionResult<ApiQuoteResponse>> Create([FromBody] ApiQuoteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var viewModel = new CreateQuoteViewModel
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerCompany = request.CustomerCompany,
            ValidUntil = request.ValidUntil ?? DateTime.UtcNow.AddDays(30),
            Currency = request.Currency,
            Notes = request.Notes,
            TemplateName = request.TemplateName,
            LineItems = request.LineItems.Select(li => new QuoteLineItemInput
            {
                ProductId = li.ProductId,
                Quantity = li.Quantity,
                DiscountPercent = li.DiscountPercent
            }).ToList()
        };

        var userId = User.Identity?.Name ?? "api-user";
        var quote = await _quoteService.CreateQuoteAsync(viewModel, userId);

        return CreatedAtAction(nameof(GetById), new { id = quote.Id }, MapToResponse(quote));
    }

    [HttpPost("{id}/preview")]
    public async Task<IActionResult> Preview(int id)
    {
        var quote = await _quoteService.GetQuoteAsync(id);
        if (quote == null)
            return NotFound();

        var pdf = await _documentService.GeneratePreviewPdfAsync(quote);
        return File(pdf, "application/pdf", $"{quote.QuoteNumber}_preview.pdf");
    }

    [HttpPost("{id}/finalize")]
    public async Task<ActionResult<ApiQuoteResponse>> Finalize(int id)
    {
        var quote = await _quoteService.GetQuoteAsync(id);
        if (quote == null)
            return NotFound();

        var userId = User.Identity?.Name ?? "api-user";
        var finalized = await _quoteService.FinalizeQuoteAsync(id, userId);

        return Ok(MapToResponse(finalized));
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var quote = await _quoteService.GetQuoteAsync(id);
        if (quote == null)
            return NotFound();

        if (string.IsNullOrEmpty(quote.GeneratedPdfPath))
            return BadRequest(new { error = "Quote has not been finalized yet." });

        var pdfBytes = await _blobStorageService.DownloadAsync("generated-pdfs", quote.GeneratedPdfPath);
        if (pdfBytes == null)
            return NotFound(new { error = "PDF file not found." });

        return File(pdfBytes, "application/pdf", $"{quote.QuoteNumber}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var quote = await _quoteService.GetQuoteAsync(id);
        if (quote == null)
            return NotFound();

        await _quoteService.DeleteQuoteAsync(id);
        return NoContent();
    }

    private static ApiQuoteResponse MapToResponse(Models.Domain.Quote q)
    {
        return new ApiQuoteResponse
        {
            Id = q.Id,
            QuoteNumber = q.QuoteNumber,
            CustomerName = q.CustomerName,
            Status = q.Status.ToString(),
            TotalAmount = q.TotalAmount,
            Currency = q.Currency,
            ValidUntil = q.ValidUntil,
            PdfDownloadUrl = !string.IsNullOrEmpty(q.GeneratedPdfPath)
                ? $"/api/quotes/{q.Id}/pdf"
                : null,
            LineItems = q.LineItems?.Select(li => new ApiLineItemResponse
            {
                ProductName = li.ProductName,
                Sku = li.Sku,
                Quantity = li.Quantity,
                CommitmentTerm = li.CommitmentTerm,
                BillingFrequency = li.BillingFrequency,
                UnitPrice = li.UnitPrice,
                DiscountPercent = li.DiscountPercent,
                LineTotal = li.LineTotal,
                Currency = li.Currency
            }).ToList() ?? new()
        };
    }
}
