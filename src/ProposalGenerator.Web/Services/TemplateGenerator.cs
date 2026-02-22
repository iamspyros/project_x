using System.Drawing;
using Aspose.Words;
using Aspose.Words.Tables;

/// <summary>
/// Generates a default Word (.docx) proposal template with mail merge fields.
/// Run this once to create the template, then customize styling in Word.
/// Usage: dotnet run -- generate-template
/// </summary>
public static class TemplateGenerator
{
    public static void GenerateDefaultTemplate(string outputPath)
    {
        var doc = new Document();
        var builder = new DocumentBuilder(doc);

        // --- Page Setup ---
        var pageSetup = builder.PageSetup;
        pageSetup.PaperSize = Aspose.Words.PaperSize.A4;
        pageSetup.TopMargin = 72; // 1 inch
        pageSetup.BottomMargin = 72;
        pageSetup.LeftMargin = 72;
        pageSetup.RightMargin = 72;

        // --- Company Logo Placeholder ---
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Right;
        builder.Font.Size = 10;
        builder.Font.Color = Color.Gray;
        builder.Writeln("[Company Logo]");
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;

        // --- Title ---
        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Title;
        builder.Font.Size = 28;
        builder.Font.Color = Color.FromArgb(0, 51, 102);
        builder.Font.Bold = true;
        builder.Writeln("PROPOSAL");

        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Subtitle;
        builder.Font.Size = 14;
        builder.Font.Color = Color.FromArgb(100, 100, 100);
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD DocumentType", "«DocumentType»");
        builder.Writeln();
        builder.Writeln();

        // --- Quote Details Section ---
        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading1;
        builder.Font.Size = 16;
        builder.Font.Color = Color.FromArgb(0, 51, 102);
        builder.Writeln("Quote Details");

        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;
        builder.Font.Size = 11;
        builder.Font.Color = Color.Black;

        // Build a details table
        var detailsTable = builder.StartTable();

        // Quote Number
        builder.InsertCell();
        builder.CellFormat.Width = 150;
        builder.CellFormat.Borders.LineStyle = LineStyle.None;
        builder.Font.Bold = true;
        builder.Write("Quote Number:");
        builder.InsertCell();
        builder.CellFormat.Width = 350;
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD QuoteNumber", "«QuoteNumber»");
        builder.EndRow();

        // Date
        builder.InsertCell();
        builder.Font.Bold = true;
        builder.Write("Date:");
        builder.InsertCell();
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD CreatedDate", "«CreatedDate»");
        builder.EndRow();

        // Valid Until
        builder.InsertCell();
        builder.Font.Bold = true;
        builder.Write("Valid Until:");
        builder.InsertCell();
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD ValidUntil", "«ValidUntil»");
        builder.EndRow();

        // Currency
        builder.InsertCell();
        builder.Font.Bold = true;
        builder.Write("Currency:");
        builder.InsertCell();
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD Currency", "«Currency»");
        builder.EndRow();

        builder.EndTable();
        builder.Writeln();

        // --- Customer Information ---
        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading1;
        builder.Font.Size = 16;
        builder.Font.Color = Color.FromArgb(0, 51, 102);
        builder.Writeln("Customer Information");

        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;
        builder.Font.Size = 11;
        builder.Font.Color = Color.Black;

        var customerTable = builder.StartTable();

        builder.InsertCell();
        builder.CellFormat.Width = 150;
        builder.CellFormat.Borders.LineStyle = LineStyle.None;
        builder.Font.Bold = true;
        builder.Write("Customer:");
        builder.InsertCell();
        builder.CellFormat.Width = 350;
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD CustomerName", "«CustomerName»");
        builder.EndRow();

        builder.InsertCell();
        builder.Font.Bold = true;
        builder.Write("Company:");
        builder.InsertCell();
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD CustomerCompany", "«CustomerCompany»");
        builder.EndRow();

        builder.InsertCell();
        builder.Font.Bold = true;
        builder.Write("Email:");
        builder.InsertCell();
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD CustomerEmail", "«CustomerEmail»");
        builder.EndRow();

        builder.EndTable();
        builder.Writeln();

        // --- Pricing Table ---
        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading1;
        builder.Font.Size = 16;
        builder.Font.Color = Color.FromArgb(0, 51, 102);
        builder.Writeln("Pricing");

        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;
        builder.Font.Size = 10;
        builder.Font.Color = Color.Black;

        var priceTable = builder.StartTable();

        // Header row
        string[] headers = { "Product", "SKU", "Qty", "Term", "Billing", "Unit Price", "Discount", "Line Total" };
        double[] widths = { 120, 80, 40, 60, 60, 70, 60, 80 };

        for (int i = 0; i < headers.Length; i++)
        {
            builder.InsertCell();
            builder.CellFormat.Width = widths[i];
            builder.CellFormat.Shading.BackgroundPatternColor = Color.FromArgb(0, 51, 102);
            builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
            builder.CellFormat.Borders.LineStyle = LineStyle.Single;
            builder.CellFormat.Borders.Color = Color.White;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            builder.Font.Color = Color.White;
            builder.Font.Bold = true;
            builder.Font.Size = 9;
            builder.Write(headers[i]);
        }
        builder.EndRow();

        // Template data row with merge region markers
        // This row will be cloned for each line item by Aspose mail merge with regions
        string[] mergeFields = { "ProductName", "Sku", "Quantity", "CommitmentTerm", "BillingFrequency", "UnitPrice", "Discount", "LineTotal" };

        builder.Font.Color = Color.Black;
        builder.Font.Bold = false;
        builder.Font.Size = 9;

        // First cell with TableStart marker
        builder.InsertCell();
        builder.CellFormat.Width = widths[0];
        builder.CellFormat.Shading.BackgroundPatternColor = Color.White;
        builder.CellFormat.Borders.LineStyle = LineStyle.Single;
        builder.CellFormat.Borders.Color = Color.FromArgb(200, 200, 200);
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        builder.InsertField("MERGEFIELD TableStart:PriceRows", "");
        builder.InsertField($"MERGEFIELD {mergeFields[0]}", $"«{mergeFields[0]}»");

        for (int i = 1; i < mergeFields.Length - 1; i++)
        {
            builder.InsertCell();
            builder.CellFormat.Width = widths[i];
            builder.CellFormat.Shading.BackgroundPatternColor = Color.White;
            builder.ParagraphFormat.Alignment = (i >= 2) ? ParagraphAlignment.Center : ParagraphAlignment.Left;
            builder.InsertField($"MERGEFIELD {mergeFields[i]}", $"«{mergeFields[i]}»");
        }

        // Last cell with TableEnd marker
        builder.InsertCell();
        builder.CellFormat.Width = widths[^1];
        builder.CellFormat.Shading.BackgroundPatternColor = Color.White;
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Right;
        builder.InsertField($"MERGEFIELD {mergeFields[^1]}", $"«{mergeFields[^1]}»");
        builder.InsertField("MERGEFIELD TableEnd:PriceRows", "");

        builder.EndRow();
        builder.EndTable();
        builder.Writeln();

        // --- Total ---
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Right;
        builder.Font.Size = 14;
        builder.Font.Bold = true;
        builder.Font.Color = Color.FromArgb(0, 51, 102);
        builder.Write("Total: ");
        builder.InsertField("MERGEFIELD TotalAmount", "«TotalAmount»");
        builder.Write(" ");
        builder.InsertField("MERGEFIELD Currency", "«Currency»");
        builder.Writeln();
        builder.Writeln();

        // --- Notes ---
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading1;
        builder.Font.Size = 16;
        builder.Font.Color = Color.FromArgb(0, 51, 102);
        builder.Writeln("Notes");

        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;
        builder.Font.Size = 11;
        builder.Font.Color = Color.Black;
        builder.Font.Bold = false;
        builder.InsertField("MERGEFIELD Notes", "«Notes»");
        builder.Writeln();
        builder.Writeln();

        // --- Footer ---
        builder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);
        builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        builder.Font.Size = 8;
        builder.Font.Color = Color.Gray;
        builder.Write("This document is confidential and intended for the named recipient only. ");
        builder.InsertField("MERGEFIELD QuoteNumber", "«QuoteNumber»");
        builder.Write(" | Page ");
        builder.InsertField("PAGE");
        builder.Write(" of ");
        builder.InsertField("NUMPAGES");

        // Save
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        doc.Save(outputPath, SaveFormat.Docx);
    }
}
