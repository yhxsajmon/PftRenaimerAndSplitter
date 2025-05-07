using System.Diagnostics;
using iText.Kernel.Pdf;

namespace PdfSplitterAndRenaimer;

public class PdfSplitter
{
    static void Main()
    {
        
        string inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input.pdf");
        
        string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputPath));
        int totalPages = pdfDoc.GetNumberOfPages();

        for (int i = 1; i <= totalPages; i++)
        {
            string outputPath = Path.Combine(outputDir, $"pagina_{i}.pdf");

            // Crea nuovo documento PDF
            PdfDocument newDoc = new PdfDocument(new PdfWriter(outputPath));
            // Usa PdfSplitter per copiare la singola pagina
            pdfDoc.CopyPagesTo(i, i, newDoc);
            newDoc.Close();
        }

        Debug.Assert(pdfDoc != null, nameof(pdfDoc) + " != null");
        pdfDoc.Close();

    Console.WriteLine($"Splittate {totalPages} pagine in {outputDir}");
    }
        
}
