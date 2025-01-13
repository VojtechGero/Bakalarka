using Azure;
using Azure.AI.DocumentIntelligence;
using System.Drawing;
using System.Text.Json;
using Tesseract;

namespace OCR;

public static class Azure
{
    public static async Task<string> UseAzure(Bitmap bitmap)
    {
        var ocrResult = await PerformOCR(bitmap);

        return ocrResult;
    }
    public static async Task<string> PerformOCR(Bitmap bitmap)
    {
        string apiKey = @"";
        string path = @"https://bakalarkaocr.cognitiveservices.azure.com";

        List<OcrPage> pages = new();
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms, format: System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            var client = new DocumentIntelligenceClient(new Uri(path), new AzureKeyCredential(apiKey));
            var token = CancellationToken.None;
            Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", BinaryData.FromStream(ms));
            AnalyzeResult result = operation.Value;

            List<OcrBox> ocrTexts = new List<OcrBox>();
            foreach (DocumentPage page in result.Pages)
            {
                float height = (float)page.Height;
                float width = (float)page.Width;
                for (int i = 0; i < page.Lines.Count; i++)
                {

                    DocumentLine line = page.Lines[i];
                    ocrTexts.Add(new OcrBox()
                    {
                        Text = line.Content,
                        Rectangle = GetBoundingRectangle(line.Polygon.ToList(), width, height)
                    });

                }
            }
            string output = JsonSerializer.Serialize(ocrTexts);
            return output;
        }

    }

    private static Pix ConvertBitmapToPix(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        ms.Position = 0;
        return Pix.LoadFromMemory(ms.ToArray());
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
        int scaledX = (int)(minX);
        int scaledY = (int)(minY);
        int scaledWidth = (int)((maxX - minX));
        int scaledHeight = (int)((maxY - minY));
        return new Rectangle(scaledX, scaledY, scaledWidth, scaledHeight);
    }
}
