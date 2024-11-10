using BakalarkaWpf.ViewModels;
using System;
using System.Windows;
using Page = System.Windows.Controls.Page;

namespace BakalarkaWpf.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        FolderTreeControl.LoadFolderStructure("./data");
        FolderTreeControl.PdfFileClicked += FolderTreeControl_PdfFileClicked;
    }

    private void FolderTreeControl_PdfFileClicked(object sender, string filePath)
    {
        PDFView.Load(filePath);
        FileMessage.Visibility = System.Windows.Visibility.Hidden;
        PerformOcrOnPdf(filePath);
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
            string ocr = Services.OcrService.RunOcrOnPdf(pdfFilePath);
            OcrOutput.Text = ocr;
            OcrOutput.Width = Double.NaN;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Clipboard.SetText(ex.Message);
        }
    }
}
