using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
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

namespace BakalarkaWpf.Views.UserControls;

/// <summary>
/// Interaction logic for PdfDisplay.xaml
/// </summary>
public partial class PdfDisplay : UserControl
{
    public FileItem fileItem;
    private OcrOverlayManager _ocrOverlayManager;
    private ApiSearchService _searchService;
    private bool DocumentLoaded = false;
    private bool overlayShowed = false;
    public EventHandler BackButtonPressed;
    private int currentResult = -1;
    private List<SearchResult> _results = new List<SearchResult>();
    public PdfDisplay(FileItem fileItem)
    {
        InitializeComponent();
        this.fileItem = fileItem;
        PDFView.ToolbarSettings.ShowZoomTools = false;
        PDFView.ToolbarSettings.ShowFileTools = false;
        PDFView.ToolbarSettings.ShowAnnotationTools = false;
        PDFView.ThumbnailSettings.IsVisible = false;
        PDFView.IsBookmarkEnabled = false;
        PDFView.EnableLayers = false;
        PDFView.PageOrganizerSettings.IsIconVisible = false;
        PDFView.EnableRedactionTool = false;
        PDFView.FormSettings.IsIconVisible = false;
        PDFView.ZoomMode = Syncfusion.Windows.PdfViewer.ZoomMode.FitWidth;
        _searchService = new ApiSearchService();
    }

    private async Task LoadFile(string pdfFilePath)
    {
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
        _ocrOverlayManager = new OcrOverlayManager(PDFView, new Pdf
        {
            Path = pdfFilePath,
            Pages = ocr,
        }, doc);
        await Task.Run(() =>
        {
            while (!DocumentLoaded) { }
        });
        PDFView.Width = CenterSegment.Width;
        CenterSegment.Children.Remove(progressBar);
        overlayShowed = true;

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
        foreach (OcrPage page in pages)
        {
            string text = string.Join("\n", page.OcrBoxes.Select(x => x.Text));
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
                Content = $"Strana {page.pageNum}"
            };
            OcrOutput.Children.Add(label);
            OcrOutput.Children.Add(textBox);
        }
    }
    private void PDFView_DocumentLoaded(object sender, EventArgs args)
    {
        DocumentLoaded = true;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        LoadFile(fileItem.Path);
    }

    private void PDFView_ZoomChanged(object sender, Syncfusion.Windows.PdfViewer.ZoomEventArgs args)
    {
        PDFView.ZoomMode = Syncfusion.Windows.PdfViewer.ZoomMode.FitWidth;
        _ocrOverlayManager?.zoomChanged();
    }

    private void PDFView_ScrollChanged(object sender, ScrollChangedEventArgs args)
    {
        _ocrOverlayManager?.HandleScroll(args);
    }


    private void OcrButton_Click(object sender, RoutedEventArgs e)
    {
        if (overlayShowed)
        {
            _ocrOverlayManager?.HideOverlay();
        }
        else
        {
            _ocrOverlayManager?.ShowOverlay();
        }
        overlayShowed = !overlayShowed;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        BackButtonPressed?.Invoke(this, EventArgs.Empty);
    }


    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Visible;
        SearchTextBox.Focus();
    }

    private async void ExecuteSearch_Click(object sender, RoutedEventArgs e)
    {
        var searchTerm = SearchTextBox.Text;
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            _results = await _searchService.GetFileResults(searchTerm, this.fileItem.Path);
            currentResult = 0;
            if (_results.Count > 0)
            {
                ResultCounter.Text = $"Výsledek {currentResult + 1} z {_results.Count}";
                highlightResult();
            }
            else
            {
                ResultCounter.Text = "No matches found.";
            }
        }
        else
        {
            ResultCounter.Text = "Please enter a search term.";
            currentResult = -1;
        }
    }
    public async void openSearchBox(FileResults fileResult)
    {
        await Task.Run(() =>
        {
            while (!DocumentLoaded) { }
        });
        SearchPanel.Visibility = Visibility.Visible;
        SearchTextBox.Text = fileResult.Query;
        if (!string.IsNullOrWhiteSpace(fileResult.Query))
        {
            _results = await _searchService.GetFileResults(fileResult.Query, this.fileItem.Path);
            currentResult = 0;
            if (_results.Count > 0)
            {
                ResultCounter.Text = $"Výsledek {currentResult + 1} z {_results.Count}";
                highlightResult();
            }
            else
            {
                ResultCounter.Text = "No matches found.";
            }
        }
        else
        {
            ResultCounter.Text = "Please enter a search term.";
            currentResult = -1;
        }
    }
    private void highlightResult()
    {
        if (currentResult != -1)
        {
            _ocrOverlayManager.Recolor(_results[currentResult]);
        }
    }
    private void NextResult_Click(object sender, RoutedEventArgs e)
    {
        currentResult++;

        if (currentResult == _results.Count)
        {
            currentResult = 0;
        }
        PDFView.GoToPageAtIndex(_results[currentResult].PageNumber);
        highlightResult();
        ResultCounter.Text = $"Výsledek {currentResult + 1} z {_results.Count}";
    }

    private void CloseSearchPanel_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Collapsed;
    }
}
