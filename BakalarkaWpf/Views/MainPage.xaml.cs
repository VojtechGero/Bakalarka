using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
using BakalarkaWpf.ViewModels;
using BakalarkaWpf.Views.UserControls;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Page = System.Windows.Controls.Page;

namespace BakalarkaWpf.Views;

public partial class MainPage : Page
{
    private string workingForlder = "./data";
    private string loadedFile = "";
    private OcrOverlayManager _ocrOverlayManager;
    private SearchService _searchService;
    private bool isSearchPanelVisible = false;
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _searchService = new SearchService(workingForlder);
        FolderTreeControl.LoadFolderStructure(workingForlder);
        FolderTreeControl.PdfFileClicked += FolderTreeControl_PdfFileClicked;
    }

    private async void FolderTreeControl_PdfFileClicked(object sender, string filePath)
    {
        await LoadAndOcr(filePath);
    }

    private async Task LoadFile(string pdfFilePath)
    {
        FileMessage.Visibility = System.Windows.Visibility.Hidden;
        if (loadedFile == "")
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
        List<OcrPage> ocr = new();
        string jsonFilePath = Path.ChangeExtension(pdfFilePath, ".json");
        if (File.Exists(jsonFilePath))
        {
            Pdf data = JsonSerializer.Deserialize<Pdf>(File.ReadAllText(jsonFilePath));
            ocr = data.Pages;
        }
        else
        {
            ocr = await Task.Run(() => PerformOcrOnPdf(pdfFilePath));
            Pdf newData = new Pdf()
            {
                Path = pdfFilePath,
                Pages = ocr
            };
            File.WriteAllText(jsonFilePath, JsonSerializer.Serialize(newData));
        }
        PdfLoadedDocument doc = new PdfLoadedDocument(pdfFilePath);

        PDFView.Load(doc);

        addOcrOutput(ocr);
        Splitter.Width = 5;
        loadedFile = pdfFilePath;
        CenterSegment.Children.Remove(progressBar);
        _ocrOverlayManager = new OcrOverlayManager(PDFView, new Pdf
        {
            Path = pdfFilePath,
            Pages = ocr
        });

        // Attach event handlers for overlay updates
        PDFView.CurrentPageChanged += (s, e) =>
            _ocrOverlayManager.RenderOcrOverlay(PDFView.CurrentPageIndex);
        PDFView.ZoomChanged += (s, e) =>
            _ocrOverlayManager.UpdateOverlayOnViewChanged();

        // Render initial overlay for first page
        _ocrOverlayManager.RenderOcrOverlay(1);
    }

    private async Task LoadAndOcr(string pdfFilePath)
    {
        await LoadFile(pdfFilePath);
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

    private List<OcrPage> PerformOcrOnPdf(string pdfFilePath)
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

        return null;
    }
    public void addOcrOutput(List<OcrPage> pages)
    {
        OcrOutput.Children.Clear();
        foreach (OcrPage page in pages)
        {
            string text = string.Join(" ", page.OcrBoxes.Select(x => x.Text));
            var textBox = new TextBox();
            textBox.Height = double.NaN;
            textBox.Width = double.NaN;
            textBox.TextWrapping = TextWrapping.Wrap;
            var label = new Label();
            label.Content = $"Page {page.pageNum}";
            OcrOutput.Children.Add(label);
            OcrOutput.Children.Add(textBox);
            textBox.Text = text;
        }
        OcrOutput.Width = double.NaN;
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        if (!isSearchPanelVisible)
        {
            // Set width to 300px (or desired width)
            SearchPanelColumn.Width = new GridLength(300);
        }
        else
        {
            // Set width to 0 to hide the panel
            SearchPanelColumn.Width = new GridLength(0);
        }

        // Toggle the visibility state
        isSearchPanelVisible = !isSearchPanelVisible;
    }

    private async void SearchFiles_Click(object sender, RoutedEventArgs e)
    {
        string query = SearchTextBox.Text;
        SearchResultPanel.Children.RemoveRange(2, SearchResultPanel.Children.Count);
        if (!string.IsNullOrWhiteSpace(query))
        {
            var results = await _searchService.SearchAsync(query);
            foreach (var result in results)
            {
                string stringResult = $"File: {result.FilePath}, Page: {result.PageNumber}, MatchIndex {result.MatchIndex}, Box {result.BoxIndex}";
                var resultDisplay = new TextBlock();
                resultDisplay.Height = double.NaN;
                resultDisplay.Width = double.NaN;
                resultDisplay.TextWrapping = TextWrapping.Wrap;
                resultDisplay.Text = stringResult;
                resultDisplay.MouseDown += (sender, e) => OpenSearchResults(sender, e, result.FilePath);
                SearchResultPanel.Children.Add(resultDisplay);
            }

        }

    }
    private async Task OpenSearchResults(object sender, MouseEventArgs e, string filepath)
    {
        await LoadFile(filepath);
    }
}
