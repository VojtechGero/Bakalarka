using BakalarkaWpf.ViewModels;
using Syncfusion.Pdf.Parsing;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using Tesseract;
using Page = System.Windows.Controls.Page;

namespace BakalarkaWpf.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void LoadButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.FileName = "Document";
        dialog.DefaultExt = ".pdf";
        dialog.Filter = "Pdf document (.pdf)|*.pdf";
        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            string filename = dialog.FileName;
            PDFView.Load(filename);
            FileMessage.Visibility = System.Windows.Visibility.Hidden;
            PerformOcrOnPdf(filename);
        }
    }
    private void PerformOcrOnPdf(string pdfFilePath)
    {
        try
        {
            // Load the PDF document
            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(pdfFilePath);
            string ocrResultText = "";

            // Set up Tesseract OCR engine (adjust paths accordingly)
            string tessDataPath = @"D:\NET\Tessdata\tessdata"; // Example: "tessdata"
            using var ocrEngine = new TesseractEngine(tessDataPath, "ces", EngineMode.Default);

            // Loop through each page
            for (int i = 0; i < loadedDocument.Pages.Count; i++)
            {
                using (var pageImage = loadedDocument.ExportAsImage(i))
                {
                    // Convert Syncfusion page image to Bitmap for Tesseract
                    using (Bitmap bitmap = new Bitmap(pageImage))
                    {
                        using var pix = ConvertBitmapToPix(bitmap);
                        using var page = ocrEngine.Process(pix);
                        ocrResultText += page.GetText();
                    }
                }
            }

            // Display OCR result
            Clipboard.SetText(ocrResultText);
            MessageBox.Show("copied to clipboard");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error performing OCR: {ex.Message}");
        }
    }
    private Pix ConvertBitmapToPix(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        ms.Position = 0;
        return Pix.LoadFromMemory(ms.ToArray());
    }
}
