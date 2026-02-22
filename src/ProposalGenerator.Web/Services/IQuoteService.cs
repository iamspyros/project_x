using ProposalGenerator.Web.Models.Domain;
using ProposalGenerator.Web.Models.ViewModels;

namespace ProposalGenerator.Web.Services;

public interface IQuoteService
{
    Task<Quote> CreateQuoteAsync(CreateQuoteViewModel model, string userId);
    Task<Quote?> GetQuoteAsync(int id);
    Task<List<Quote>> GetAllQuotesAsync();
    Task<Quote> FinalizeQuoteAsync(int quoteId, string userId);
    Task DeleteQuoteAsync(int quoteId);
    string GenerateQuoteNumber();
}
