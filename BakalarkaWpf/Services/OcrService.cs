using BakalarkaWpf.Models;
using Syncfusion.Pdf.Parsing;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
namespace BakalarkaWpf.Services;

public class OcrService
{
    public static List<OcrPage> RunOcrOnPdf(string pdfFilePath)
    {
        PdfLoadedDocument loadedDocument = new PdfLoadedDocument(pdfFilePath);
        List<OcrPage> pages = new List<OcrPage>();

        // Loop through each page
        for (int i = 0; i < loadedDocument.Pages.Count; i++)
        {
            using (var pageImage = loadedDocument.ExportAsImage(i))
            {
                List<OcrBox> boxes = JsonSerializer.Deserialize<List<OcrBox>>(runOcr(pageImage));
                pages.Add(new OcrPage()
                {
                    OcrBoxes = boxes,
                    pageNum = i + 1,
                });
            }
        }
        return pages;
    }


    public static string runOcr(Bitmap page)
    {
        return OCR.Tesseract.UseTesseract(page);
    }
}
