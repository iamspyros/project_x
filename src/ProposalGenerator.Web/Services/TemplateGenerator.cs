/// <summary>
/// Placeholder kept for backward compatibility.
/// PDF generation is now handled entirely by QuestPDF in DocumentService.
/// </summary>
public static class TemplateGenerator
{
    /// <summary>
    /// No-op: .docx template generation is no longer needed because
    /// QuestPDF generates branded PDFs directly in code.
    /// </summary>
    public static void GenerateDefaultTemplate(string outputPath)
    {
        // Nothing to generate — QuestPDF templates are code-based.
    }
}
