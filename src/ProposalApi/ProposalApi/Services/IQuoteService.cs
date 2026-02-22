using ProposalApi.Models;
using ProposalApi.Models.Dto;

namespace ProposalApi.Services;

public interface IQuoteService
{
    Task<(Quote quote, byte[] pdf)> GeneratePreviewAsync(QuotePreviewRequest request, string userId, string userName);
    Task<(Quote quote, byte[] pdf, string downloadUrl)> FinalizeAsync(QuoteFinalizeRequest request, string userId, string userName);
    Task<Quote?> GetQuoteAsync(int quoteId);
    Task<List<Quote>> GetUserQuotesAsync(string userId);
}
