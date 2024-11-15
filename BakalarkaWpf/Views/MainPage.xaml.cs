using BakalarkaWpf.Models;
using BakalarkaWpf.ViewModels;
using BakalarkaWpf.Views.UserControls;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Page = System.Windows.Controls.Page;

namespace BakalarkaWpf.Views;

public partial class MainPage : Page
{
    private string workingForlder = "./data";
    private bool pdfLoaded = false;
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        FolderTreeControl.LoadFolderStructure(workingForlder);
        FolderTreeControl.PdfFileClicked += FolderTreeControl_PdfFileClicked;
    }

    private async void FolderTreeControl_PdfFileClicked(object sender, string filePath)
    {
        await LoadAndOcr(filePath);
    }
    private async Task LoadAndOcr(string pdfFilePath)
    {
        FileMessage.Visibility = System.Windows.Visibility.Hidden;
        if (pdfLoaded)
        {
            OcrOutput.Width = 0;
            Splitter.Width = 0;
            PDFView.Unload();
        }
        var progressBar = new MyProgressBar(Path.GetFileName(pdfFilePath))
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        CenterSegment.Children.Add(progressBar);

        await Task.Delay(50);
        string ocr = "";
        string jsonFilePath = Path.ChangeExtension(pdfFilePath, ".json");
        if (File.Exists(jsonFilePath))
        {
            Pdf data = JsonSerializer.Deserialize<Pdf>(File.ReadAllText(jsonFilePath));
            ocr = data.Content;
        }
        else
        {
            ocr = await Task.Run(() => PerformOcrOnPdf(pdfFilePath));
            Pdf newData = new Pdf()
            {
                Path = pdfFilePath,
                Content = ocr
            };
            File.WriteAllText(jsonFilePath, JsonSerializer.Serialize(newData));
        }
        await PDFView.LoadAsync(pdfFilePath);
        OcrOutput.Text = ocr;
        OcrOutput.Width = double.NaN;
        Splitter.Width = 5;
        pdfLoaded = true;
        CenterSegment.Children.Remove(progressBar);

    }
    private async void LoadButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            FileName = "Document",
            DefaultExt = ".pdf",
            Filter = "Pdf document (.pdf)|*.pdf"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            string localPath = copyFile(dialog.FileName);

            await LoadAndOcr(localPath);

        }
    }

    private string copyFile(string filePath)
    {
        string name = Path.GetFileName(filePath);
        string newPath = Path.Combine(workingForlder, name);
        File.Copy(filePath, newPath);
        FolderTreeControl.Update();
        return newPath;
    }

    private string PerformOcrOnPdf(string pdfFilePath)
    {
        try
        {
            return Services.OcrService.RunOcrOnPdf(pdfFilePath);
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(ex.Message);
                Clipboard.SetText(ex.Message);
            });
        }

        return "";
    }
}
