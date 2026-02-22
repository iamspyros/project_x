using ProposalGenerator.Web.Models.ViewModels;

namespace ProposalGenerator.Web.Services;

public interface IPriceImportService
{
    Task<List<string>> GetAvailableFilesAsync();
    Task<PriceImportResultViewModel> ImportFileAsync(string fileName);
    Task<PriceImportResultViewModel> ImportUploadedFileAsync(Stream fileStream, string fileName);
}
