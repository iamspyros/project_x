using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Models.Domain;
using ProposalGenerator.Web.Models.ViewModels;

namespace ProposalGenerator.Web.Services;

public class PriceImportService : IPriceImportService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PriceImportService> _logger;

    public PriceImportService(
        AppDbContext db,
        IAuditService audit,
        IConfiguration configuration,
        ILogger<PriceImportService> logger)
    {
        _db = db;
        _audit = audit;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<List<string>> GetAvailableFilesAsync()
    {
        var folderPath = _configuration["PriceImport:FolderPath"] ?? "./price-import";
        var files = new List<string>();

        if (Directory.Exists(folderPath))
        {
            var allowedExtensions = _configuration.GetSection("PriceImport:AllowedExtensions")
                .Get<string[]>() ?? new[] { ".csv" };

            files.AddRange(
                Directory.GetFiles(folderPath)
                    .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .Select(Path.GetFileName)!);
        }

        return Task.FromResult(files);
    }

    public async Task<PriceImportResultViewModel> ImportFileAsync(string fileName)
    {
        var folderPath = _configuration["PriceImport:FolderPath"] ?? "./price-import";
        var filePath = Path.Combine(folderPath, fileName);

        if (!File.Exists(filePath))
        {
            return new PriceImportResultViewModel
            {
                FileName = fileName,
                Errors = { $"File not found: {fileName}" }
            };
        }

        using var stream = File.OpenRead(filePath);
        return await ImportFromStreamAsync(stream, fileName);
    }

    public async Task<PriceImportResultViewModel> ImportUploadedFileAsync(Stream fileStream, string fileName)
    {
        return await ImportFromStreamAsync(fileStream, fileName);
    }

    private async Task<PriceImportResultViewModel> ImportFromStreamAsync(Stream stream, string fileName)
    {
        var result = new PriceImportResultViewModel { FileName = fileName };

        try
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim
            });

            var records = csv.GetRecords<PriceImportRow>().ToList();
            result.TotalRows = records.Count;

            foreach (var row in records)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(row.Sku))
                    {
                        result.Skipped++;
                        continue;
                    }

                    var existing = _db.Products.FirstOrDefault(p => p.Sku == row.Sku);
                    if (existing != null)
                    {
                        existing.Name = row.ProductName ?? existing.Name;
                        existing.Description = row.Description ?? existing.Description;
                        existing.Category = row.Category ?? existing.Category;
                        existing.UnitPrice = row.UnitPrice;
                        existing.Currency = row.Currency ?? existing.Currency;
                        existing.CommitmentTerm = row.CommitmentTerm ?? existing.CommitmentTerm;
                        existing.BillingFrequency = row.BillingFrequency ?? existing.BillingFrequency;
                        existing.IsActive = true;
                        existing.UpdatedAt = DateTime.UtcNow;
                        result.Updated++;
                    }
                    else
                    {
                        _db.Products.Add(new Product
                        {
                            Name = row.ProductName ?? row.Sku,
                            Sku = row.Sku,
                            Description = row.Description,
                            Category = row.Category,
                            UnitPrice = row.UnitPrice,
                            Currency = row.Currency ?? "EUR",
                            CommitmentTerm = row.CommitmentTerm,
                            BillingFrequency = row.BillingFrequency,
                            IsActive = true
                        });
                        result.Imported++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row SKU '{row.Sku}': {ex.Message}");
                    result.Skipped++;
                }
            }

            await _db.SaveChangesAsync();
            await _audit.LogAsync("PriceImport", "Product", null, null,
                $"File: {fileName}, Imported: {result.Imported}, Updated: {result.Updated}, Skipped: {result.Skipped}");

            _logger.LogInformation(
                "Price import {File}: {Imported} imported, {Updated} updated, {Skipped} skipped",
                fileName, result.Imported, result.Updated, result.Skipped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import price file {File}", fileName);
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }
}

internal class PriceImportRow
{
    public string? ProductName { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Currency { get; set; }
    public string? CommitmentTerm { get; set; }
    public string? BillingFrequency { get; set; }
}
