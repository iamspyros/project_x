using ProposalGenerator.Web.Models.Domain;

namespace ProposalGenerator.Web.Services;

public interface IDocumentService
{
    Task<byte[]> GeneratePreviewPdfAsync(Quote quote);
    Task<byte[]> GenerateFinalPdfAsync(Quote quote);
    Task<List<string>> GetAvailableTemplatesAsync();
}
