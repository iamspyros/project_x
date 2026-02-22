using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProposalGenerator.Web.Models.ViewModels;
using ProposalGenerator.Web.Services;

namespace ProposalGenerator.Web.Pages.PriceImport;

public class IndexModel : PageModel
{
    private readonly IPriceImportService _importService;

    public IndexModel(IPriceImportService importService)
    {
        _importService = importService;
    }

    public List<string> AvailableFiles { get; set; } = new();
    public PriceImportResultViewModel? ImportResult { get; set; }

    public async Task OnGetAsync()
    {
        AvailableFiles = await _importService.GetAvailableFilesAsync();
    }

    public async Task<IActionResult> OnPostUploadAsync(IFormFile csvFile)
    {
        AvailableFiles = await _importService.GetAvailableFilesAsync();

        if (csvFile == null || csvFile.Length == 0)
        {
            TempData["Error"] = "Please select a CSV file to upload.";
            return Page();
        }

        if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Only CSV files are accepted.";
            return Page();
        }

        try
        {
            using var stream = csvFile.OpenReadStream();
            ImportResult = await _importService.ImportUploadedFileAsync(stream, csvFile.FileName);

            if (ImportResult.Success)
            {
                TempData["Success"] = $"Successfully imported {ImportResult.Imported} new and updated {ImportResult.Updated} existing products from {csvFile.FileName}.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Import failed: {ex.Message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostImportFileAsync(string fileName)
    {
        AvailableFiles = await _importService.GetAvailableFilesAsync();

        if (string.IsNullOrEmpty(fileName))
        {
            TempData["Error"] = "Please select a file to import.";
            return Page();
        }

        try
        {
            ImportResult = await _importService.ImportFileAsync(fileName);

            if (ImportResult.Success)
            {
                TempData["Success"] = $"Successfully imported {ImportResult.Imported} new and updated {ImportResult.Updated} existing products from {fileName}.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Import failed: {ex.Message}";
        }

        return Page();
    }
}
