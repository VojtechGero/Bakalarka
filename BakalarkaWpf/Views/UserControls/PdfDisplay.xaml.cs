using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace BakalarkaWpf.Views.UserControls;

/// <summary>
/// Interaction logic for PdfDisplay.xaml
/// </summary>
public partial class PdfDisplay : UserControl
{
    public FileItem _fileItem;
    private OcrOverlayManager _ocrOverlayManager;
    private ApiSearchService _searchService;
    private ApiFileService _fileService;
    private bool _isOverlayShown = false;
    public EventHandler BackButtonPressed;
    private int _currentSearchResultIndex = -1;
    private List<SearchResult> _searchResults = new();
    public PdfDisplay(FileItem fileItem)
    {
        InitializeComponent();
        _fileItem = fileItem;
        InitializePdfViewerSettings();

        _searchService = new ApiSearchService();
        _fileService = new ApiFileService();
    }

    private void InitializePdfViewerSettings()
    {
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
    }


    private async Task LoadDocumentAsync(string pdfFilePath)
    {
        var progressBar = new MyProgressBar
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        progressBar.UpdateMessage($"Načítání dokumentu: {Path.GetFileName(pdfFilePath)}");
        CenterSegment.Children.Add(progressBar);

        try
        {
            var stream = await _fileService.GetFileAsync(pdfFilePath);
            var document = new PdfLoadedDocument(stream);
            var ocrData = await GetOcrDataAsync(pdfFilePath);

            PDFView.Load(document);
            UpdateOcrResultsPanel(ocrData.Pages);

            _ocrOverlayManager = new OcrOverlayManager(PDFView, ocrData, document);
        }
        finally
        {
            CenterSegment.Children.Remove(progressBar);
            _isOverlayShown = true;
        }
    }
    private async Task<Pdf> GetOcrDataAsync(string pdfFilePath)
    {
        try
        {
            return await _fileService.GetOcrAsync(pdfFilePath, 0, 0);
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show($"Chyba OCR: {ex.Message}"));
            return null;
        }
    }
    private void UpdateOcrResultsPanel(IEnumerable<OcrPage> pages)
    {
        OcrOutput.Children.Clear();

        foreach (var page in pages)
        {
            var pageText = string.Join("\n", page.OcrBoxes.Select(b => b.Text));

            OcrOutput.Children.Add(new Label { Content = $"Strana {page.pageNum}" });

            var textBox = new TextBox
            {
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                Text = pageText
            };

            textBox.SetBinding(WidthProperty, new Binding("ActualWidth")
            {
                Source = OcrOutput,
                Mode = BindingMode.OneWay
            });

            OcrOutput.Children.Add(textBox);
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        LoadDocumentAsync(_fileItem.Path);
    }

    private void PDFView_ZoomChanged(object sender, Syncfusion.Windows.PdfViewer.ZoomEventArgs args)
    {
        _ocrOverlayManager?.zoomChanged();
    }

    private void PDFView_ScrollChanged(object sender, ScrollChangedEventArgs args)
    {
        _ocrOverlayManager?.HandleScroll(args);
    }

    private void OcrButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isOverlayShown)
        {
            _ocrOverlayManager?.HideOverlay();
        }
        else
        {
            _ocrOverlayManager?.ShowOverlay();
        }
        _isOverlayShown = !_isOverlayShown;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        BackButtonPressed?.Invoke(this, EventArgs.Empty);
    }



    private async Task PerformSearchAsync()
    {
        var searchTerm = SearchTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(searchTerm)) return;

        _searchResults = await _searchService.GetFileResults(searchTerm, _fileItem.Path);
        _currentSearchResultIndex = _searchResults.Count > 0 ? 0 : -1;
        UpdateSearchResultsDisplay();
    }
    private void UpdateSearchResultsDisplay()
    {
        if (_currentSearchResultIndex == -1)
        {
            ResultCounter.Text = "Nenalezeny žádné výsledky";
            return;
        }
        ResultCounter.Text = $"Výsledek {_currentSearchResultIndex + 1} z {_searchResults.Count}";
        HighlightAndScrollToCurrentResult();
    }


    private void HighlightAndScrollToCurrentResult()
    {
        if (_currentSearchResultIndex != -1)
        {
            var result = _searchResults[_currentSearchResultIndex];
            _ocrOverlayManager.Recolor(result);
            var position = _ocrOverlayManager.GetBoxVerticalOffset(result);
            PDFView.ScrollTo(position / ((double)PDFView.ZoomPercentage / 100d) - 200);

            var textbox = (TextBox)OcrOutput.Children[(result.PageNumber - 1) * 2 + 1];
            textbox.Focus();
            textbox.Select(result.MatchIndex, result.MatchedText.Length);
        }
    }
    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Visible;
        SearchTextBox.Focus();
    }
    private async void ExecuteSearch_Click(object sender, RoutedEventArgs e)
    {
        await PerformSearchAsync();
    }
    private void NextResult_Click(object sender, RoutedEventArgs e)
    {
        NavigateToSearchResult(1);
    }
    private void PreviousSearchPanel_Click(object sender, RoutedEventArgs e)
    {
        NavigateToSearchResult(-1);
    }
    private void CloseSearchPanel_Click(object sender, RoutedEventArgs e)
    {
        SearchPanel.Visibility = Visibility.Collapsed;
        _ocrOverlayManager.ClearColors();
    }
    private async void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await PerformSearchAsync();
        }
    }
    public async void openSearchBox(FileResults fileResult)
    {
        SearchPanel.Visibility = Visibility.Visible;
        SearchTextBox.Text = fileResult.Query;
        if (!string.IsNullOrWhiteSpace(fileResult.Query))
        {
            _searchResults = await _searchService.GetFileResults(fileResult.Query, this._fileItem.Path);
            _currentSearchResultIndex = 0;
            if (_searchResults.Count > 0)
            {
                ResultCounter.Text = $"Výsledek {_currentSearchResultIndex + 1} z {_searchResults.Count}";
                HighlightAndScrollToCurrentResult();
            }
            else
            {
                ResultCounter.Text = "Nic nebylo nalezeno.";
            }
        }
        else
        {
            ResultCounter.Text = "Zadejte frázi.";
            _currentSearchResultIndex = -1;
        }
    }
    private void NavigateToSearchResult(int direction)
    {
        if (_searchResults.Count == 0) return;

        _currentSearchResultIndex = (_currentSearchResultIndex + direction + _searchResults.Count) % _searchResults.Count;
        UpdateSearchResultsDisplay();
    }
}
