using System.Drawing;
using Tesseract;

namespace OCR;
public static class Tesseract
{
    public static string UseTesseract(Bitmap bitmap)
    {


        if (bitmap == null)
        {
            return null;
        }

        string ocrResult = PerformOCR(bitmap);

        return ocrResult;
    }

    // Method to convert a base64 string to a Bitmap
    private static Bitmap ConvertBase64ToBitmap(string base64String)
    {
        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new System.IO.MemoryStream(imageBytes))
            {
                return new Bitmap(ms);
            }
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    // Method to perform OCR using Tesseract
    private static string PerformOCR(Bitmap bitmap)
    {
        try
        {
            // Initialize Tesseract engine
            string tessDataPath = @"../../../../OCR/tessdata"; // Path to your tessdata folder
            //return Directory.GetCurrentDirectory();
            using (var engine = new TesseractEngine(tessDataPath, "ces", EngineMode.Default))
            {
                // Convert the bitmap to Tesseract pix format
                using (var pix = ConvertBitmapToPix(bitmap))
                {
                    // Perform OCR
                    var result = engine.Process(pix);
                    return result.GetText(); // Return the recognized text
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error processing image: {ex.Message}";
        }
    }
    private static Pix ConvertBitmapToPix(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        ms.Position = 0;
        return Pix.LoadFromMemory(ms.ToArray());
    }
}