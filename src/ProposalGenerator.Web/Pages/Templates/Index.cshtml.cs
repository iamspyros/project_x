using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProposalGenerator.Web.Services;

namespace ProposalGenerator.Web.Pages.Templates;

public class IndexModel : PageModel
{
    private readonly IDocumentService _documentService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IConfiguration _configuration;

    public IndexModel(
        IDocumentService documentService,
        IBlobStorageService blobStorageService,
        IConfiguration configuration)
    {
        _documentService = documentService;
        _blobStorageService = blobStorageService;
        _configuration = configuration;
    }

    public List<string> Templates { get; set; } = new();

    public async Task OnGetAsync()
    {
        Templates = await _documentService.GetAvailableTemplatesAsync();
    }

    public async Task<IActionResult> OnPostUploadAsync(IFormFile templateFile)
    {
        if (templateFile == null || templateFile.Length == 0)
        {
            TempData["Error"] = "Please select a template file to upload.";
            return RedirectToPage();
        }

        if (!templateFile.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
            && !templateFile.FileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Only Word documents (.docx, .doc) are accepted.";
            return RedirectToPage();
        }

        try
        {
            // Save to templates folder
            var templatesPath = _configuration["Templates:FolderPath"] ?? "templates";

            if (!Directory.Exists(templatesPath))
            {
                Directory.CreateDirectory(templatesPath);
            }

            var filePath = Path.Combine(templatesPath, templateFile.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await templateFile.CopyToAsync(stream);
            }

            TempData["Success"] = $"Template '{templateFile.FileName}' uploaded successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error uploading template: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            TempData["Error"] = "No file specified.";
            return RedirectToPage();
        }

        try
        {
            var templatesPath = _configuration["Templates:FolderPath"] ?? "templates";
            var filePath = Path.Combine(templatesPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["Success"] = $"Template '{fileName}' deleted.";
            }
            else
            {
                TempData["Error"] = $"Template '{fileName}' not found.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting template: {ex.Message}";
        }

        return RedirectToPage();
    }
}
