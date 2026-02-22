using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.EntityFrameworkCore;
using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Authentication ---
var authEnabled = builder.Configuration.GetValue<bool>("Authentication:Enabled");
if (authEnabled)
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = options.DefaultPolicy;
    });

    builder.Services.AddRazorPages()
        .AddMicrosoftIdentityUI();
}
else
{
    builder.Services.AddRazorPages();
}

// --- Database ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=proposalgenerator.db"));
}
else if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase)
      || connectionString.Contains("Data Source=tcp:", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));
}

// --- Services ---
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IPriceImportService, PriceImportService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// --- API Controllers ---
builder.Services.AddControllers();

// --- CORS for Flutter app ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterApp", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        policy.WithOrigins(origins ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// --- Aspose license ---
var asposeLicensePath = builder.Configuration["Aspose:LicensePath"];
if (!string.IsNullOrEmpty(asposeLicensePath) && File.Exists(asposeLicensePath))
{
    var license = new Aspose.Words.License();
    license.SetLicense(asposeLicensePath);
}

var app = builder.Build();

// --- Initialize database ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

// Generate default Word template if none exists
var templatesPath = app.Configuration["Templates:FolderPath"] ?? "./templates";
var defaultTemplatePath = Path.Combine(templatesPath, "default-proposal.docx");
if (!File.Exists(defaultTemplatePath))
{
    try
    {
        TemplateGenerator.GenerateDefaultTemplate(defaultTemplatePath);
        app.Logger.LogInformation("Generated default proposal template at {Path}", defaultTemplatePath);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not generate default template (Aspose license may be required)");
    }
}

// Ensure price-import folder exists
var priceImportPath = app.Configuration["PriceImport:FolderPath"] ?? "./price-import";
if (!Directory.Exists(priceImportPath))
{
    Directory.CreateDirectory(priceImportPath);
}

// Ensure local storage folder exists
var storagePath = app.Configuration["BlobStorage:LocalBasePath"] ?? "./storage";
if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Only redirect to HTTPS if the app is listening on an HTTPS port
if (app.Urls.Any(u => u.StartsWith("https", StringComparison.OrdinalIgnoreCase))
    || app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("FlutterApp");

if (authEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapRazorPages();
app.MapControllers();

app.Run();
