using BakalarkaWpf.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
namespace BakalarkaWpf.Services;

public class OcrService
{
    public static async Task<List<OcrPage>> RunOcrOnPdf(string pdfFilePath)
    {
        List<OcrPage> pages = new List<OcrPage>();
        pages = JsonSerializer.Deserialize<List<OcrPage>>(await runOcrAzuze(pdfFilePath));
        return pages;
    }


    public static string runOcr(Bitmap page)
    {
        return OCR.Tesseract.UseTesseract(page);
    }

    public static async Task<string> runOcrAzuze(string filePath)
    {
        return await OCR.Azure.UseAzure(filePath);
    }
}
