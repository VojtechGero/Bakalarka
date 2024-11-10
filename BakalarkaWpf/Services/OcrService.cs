using Syncfusion.Pdf.Parsing;
using System.Drawing;
namespace BakalarkaWpf.Services;

public class OcrService
{
    public static string RunOcrOnPdf(string pdfFilePath)
    {
        PdfLoadedDocument loadedDocument = new PdfLoadedDocument(pdfFilePath);
        string ocrResultText = "";


        // Loop through each page
        for (int i = 0; i < loadedDocument.Pages.Count; i++)
        {
            using (var pageImage = loadedDocument.ExportAsImage(i))
            {
                ocrResultText += "\n" + runOcr(pageImage);
            }
        }
        return ocrResultText;
    }


    public static string runOcr(Bitmap page)
    {
        return OCR.Tesseract.UseTesseract(page);
    }
}
