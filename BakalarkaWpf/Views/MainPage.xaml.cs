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
using System.Windows.Data;
using System.Windows.Input;
using Page = System.Windows.Controls.Page;

namespace BakalarkaWpf.Views;

public partial class MainPage : Page
{
    private string workingForlder;
    private string loadedFile = "";
    private OcrOverlayManager _ocrOverlayManager;
    private SearchService _searchService;
    private bool isSearchPanelVisible = false;
    private bool DocumentLoaded = false;
    private bool overlayShowed = false;
    private List<SearchResult> searchResults = new List<SearchResult>();
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        workingForlder = Path.GetFullPath("./data");
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
        if (pdfFilePath == loadedFile) return;
        FileMessage.Visibility = System.Windows.Visibility.Hidden;
        if (loadedFile == "")
        {
            OcrOutput.Width = 0;
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
        DocumentLoaded = false;
        if (File.Exists(jsonFilePath))
        {
            Pdf data = JsonSerializer.Deserialize<Pdf>(File.ReadAllText(jsonFilePath));
            ocr = data.Pages;
        }
        else
        {
            ocr = await PerformOcrOnPdf(pdfFilePath);
            Pdf newData = new Pdf()
            {
                Path = pdfFilePath,
                Pages = ocr
            };
            File.WriteAllText(jsonFilePath, JsonSerializer.Serialize(newData));
        }
        PdfLoadedDocument doc = new PdfLoadedDocument(pdfFilePath);
        addOcrOutput(ocr);
        PDFView.Load(doc);
        await Task.Run(() =>
        {
            while (!DocumentLoaded) { }
        });
        PDFView.Width = CenterSegment.Width;
        loadedFile = pdfFilePath;
        CenterSegment.Children.Remove(progressBar);
        _ocrOverlayManager = new OcrOverlayManager(PDFView, new Pdf
        {
            Path = pdfFilePath,
            Pages = ocr
        });
        overlayShowed = true;

        PDFView.CurrentPageChanged += (s, e) => _ocrOverlayManager.RenderOcrOverlay(PDFView.CurrentPageIndex);

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

    private Task<List<OcrPage>> PerformOcrOnPdf(string pdfFilePath)
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
        ShowButton.Visibility = Visibility.Visible;
        OcrOutput.Visibility = Visibility.Visible;
        OcrOutput.Width = 300;
        foreach (OcrPage page in pages)
        {
            string text = string.Join(" ", page.OcrBoxes.Select(x => x.Text));
            var textBox = new TextBox
            {
                Height = double.NaN,
                Width = double.NaN,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Text = text
            };
            Binding widthBinding = new Binding
            {
                Source = OcrOutput,
                Path = new PropertyPath("ActualWidth"),
                Mode = BindingMode.OneWay
            };
            textBox.SetBinding(TextBox.WidthProperty, widthBinding);
            var label = new Label
            {
                Content = $"Page {page.pageNum}"
            };
            OcrOutput.Children.Add(label);
            OcrOutput.Children.Add(textBox);
        }
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        if (!isSearchPanelVisible)
        {
            SearchPanelColumn.Width = new GridLength(300);
        }
        else
        {
            SearchPanelColumn.Width = new GridLength(0);
        }

        isSearchPanelVisible = !isSearchPanelVisible;
    }

    private async void SearchFiles_Click(object sender, RoutedEventArgs e)
    {
        string query = SearchTextBox.Text;
        SearchResultPanel.Children.RemoveRange(2, SearchResultPanel.Children.Count);
        if (!string.IsNullOrWhiteSpace(query))
        {
            searchResults.Clear();
            searchResults = await _searchService.SearchAsync(query);
            var fileResults = searchResults
            .GroupBy(sr => sr.FilePath)
            .Select(group => new
            {
                FileName = group.Key,
                OccurrenceCount = group.Count()
            })
            .ToList();
            foreach (var result in fileResults)
            {
                string stringResult = $"File: {Path.GetFileName(result.FileName)}, Number of occurrences: {result.OccurrenceCount}";
                var resultDisplay = new TextBlock();
                resultDisplay.Height = double.NaN;
                resultDisplay.Width = double.NaN;
                resultDisplay.TextWrapping = TextWrapping.Wrap;
                resultDisplay.Text = stringResult;
                resultDisplay.MouseDown += (sender, e) => OpenSearchResults(sender, e, result.FileName);
                resultDisplay.MouseEnter += HighlightResult;
                resultDisplay.MouseLeave += RemoveHighlight;
                resultDisplay.Cursor = Cursors.Hand;
                SearchResultPanel.Children.Add(resultDisplay);
            }

        }

    }

    private void HighlightResult(object sender, MouseEventArgs e)
    {
        TextBlock block = (TextBlock)sender;
        block.Background = System.Windows.Media.Brushes.Aqua;
    }
    private void RemoveHighlight(object sender, MouseEventArgs e)
    {
        TextBlock block = (TextBlock)sender;
        block.Background = System.Windows.Media.Brushes.White;
    }

    private async Task OpenSearchResults(object sender, MouseEventArgs e, string Filename)
    {
        await LoadFile(Filename);
        var resultControl = Container.Children.OfType<SearchResultNavigation>().FirstOrDefault();
        var relevantResults = searchResults.Where(x => x.FilePath == Filename).ToList();
        if (resultControl is not null)
        {
            Container.Children.Remove(resultControl);
        }
        resultControl = new SearchResultNavigation(relevantResults, this);
        Container.Children.Add(resultControl);
        await NextResult(relevantResults.First());
    }
    public async Task NextResult(SearchResult result)
    {
        PDFView.GoToPageAtIndex(result.PageNumber - 1);
        await Task.Delay(50);
        _ocrOverlayManager.Recolor(result);
    }
    private void PDFView_DocumentLoaded(object sender, EventArgs args)
    {
        DocumentLoaded = true;
    }

    private void ShowButton_Click(object sender, RoutedEventArgs e)
    {
        if (overlayShowed)
        {
            _ocrOverlayManager.hideOverlay();
        }
        else
        {
            _ocrOverlayManager.showOverlay();
        }
        overlayShowed = !overlayShowed;
    }

    private void PDFView_ScrollChanged(object sender, ScrollChangedEventArgs args)
    {
        if (_ocrOverlayManager != null)
        {
            //_ocrOverlayManager.HandleScroll(args);
        }
    }
}
