using Azure;
using Azure.AI.DocumentIntelligence;
using System.Drawing;
using System.Text.Json;

namespace OCR;

public static class Azure
{
    public static async Task<string> UseAzure(string filePath)
    {
        var ocrResult = await PerformOCR(filePath);

        return ocrResult;
    }
    public static async Task<string> PerformOCR(string pdfPath)
    {
        string apiKey = @"";
        string path = @"https://bakalarkaocr.cognitiveservices.azure.com";

        List<OcrPage> pages = new();
        using (FileStream fileStream = new FileStream(pdfPath, FileMode.Open))
        {
            var client = new DocumentIntelligenceClient(new Uri(path), new AzureKeyCredential(apiKey));
            var token = CancellationToken.None;
            Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", BinaryData.FromStream(fileStream));
            AnalyzeResult result = operation.Value;

            foreach (DocumentPage page in result.Pages)
            {
                float height = (float)page.Height;
                float width = (float)page.Width;
                List<OcrBox> ocrTexts = new List<OcrBox>();
                for (int i = 0; i < page.Lines.Count; i++)
                {

                    DocumentLine line = page.Lines[i];
                    ocrTexts.Add(new OcrBox()
                    {
                        Text = line.Content,
                        Rectangle = GetBoundingRectangle(line.Polygon.ToList(), width, height)
                    });

                }
                pages.Add(new OcrPage()
                {
                    OcrBoxes = ocrTexts,
                    pageNum = page.PageNumber,
                });
            }
        }
        string output = JsonSerializer.Serialize(pages);
        return output;
    }
    private static Rectangle GetBoundingRectangle(List<float> polygon, float pageWidth, float pageHeight)
    {
        if (polygon.Count % 2 != 0)
        {
            throw new ArgumentException("Invalid polygon data. Points should be in pairs.");
        }
        float minX = int.MaxValue;
        float minY = int.MaxValue;
        float maxX = int.MinValue;
        float maxY = int.MinValue;
        for (int i = 0; i < polygon.Count; i += 2)
        {
            float x = polygon[i];
            float y = polygon[i + 1];
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }
        float scaleX = 100 / pageWidth;
        float scaleY = 100 / pageHeight;
        float scale = scaleX * scaleY;
        int scaledX = (int)(minX * scale);
        int scaledY = (int)(minY * scale);
        int scaledWidth = (int)((maxX - minX) * scale);
        int scaledHeight = (int)((maxY - minY) * scale);
        return new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight);
    }
}
