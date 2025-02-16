using BakalarkaWpf.Models;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Windows.PdfViewer;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BakalarkaWpf.Services;

public class OcrOverlayManager
{
    private readonly Canvas _overlayCanvas;
    private readonly PdfViewerControl _pdfView;
    private readonly Pdf _pdfDocument;
    private readonly PdfLoadedDocument _loadedDocument;
    private double _horizontalOffset;
    private double _verticalOffset;
    private double _totalDocumentHeight;
    private double _pageGap = 2.2;
    private Rect _clientRect;
    private double _viewportWidth;
    private double _viewportHeight;

    public OcrOverlayManager(PdfViewerControl pdfView, Pdf pdfDocument, PdfLoadedDocument loadedDocument)
    {
        _pdfView = pdfView;
        _pdfDocument = pdfDocument;
        _loadedDocument = loadedDocument;
        _overlayCanvas = new Canvas
        {
            IsHitTestVisible = false,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        var pdfDocumentView = FindChild<PdfDocumentView>(_pdfView);
        var grid = pdfDocumentView.Parent as Grid;
        if (grid != null)
        {
            grid.ClipToBounds = false;
            var existingOverlay = grid.Children.OfType<Canvas>()
                .FirstOrDefault(c => c.Background == Brushes.Transparent);

            if (existingOverlay != null)
                grid.Children.Remove(existingOverlay);

            grid.Children.Add(_overlayCanvas);
        }
        _clientRect = _pdfView.ClientRectangle;
        _overlayCanvas.Margin = new Thickness(_clientRect.Left, 0, 0, 0);
        _viewportWidth = _clientRect.Width;
        _viewportHeight = _clientRect.Height;
        CalculateDocumentHeight();
        RenderAllPages();
    }

    private void CalculateDocumentHeight()
    {
        double sum = 0;
        double zoomFactor = _pdfView.ZoomPercentage / 100.0;
        for (int i = 0; i < _loadedDocument.Pages.Count; i++)
        {
            sum += _loadedDocument.Pages[i].Size.Height * zoomFactor + _pageGap;
        }
        _totalDocumentHeight = sum;
    }

    public void HideOverlay() => _overlayCanvas.Visibility = Visibility.Hidden;

    public void ShowOverlay()
    {
        _overlayCanvas.Visibility = Visibility.Visible;
        RenderAllPages();
    }

    private void RenderAllPages()
    {
        _overlayCanvas.Children.Clear();
        double zoomFactor = _pdfView.ZoomPercentage / 100.0;
        double verticalOffset = 0;

        foreach (var page in _pdfDocument.Pages.OrderBy(p => p.pageNum))
        {
            var pdfPage = _loadedDocument.Pages[page.pageNum - 1];
            // Convert PDF points to WPF DIPs (1 point = 1.3333 DIPs)
            double pageHeightDip = pdfPage.Size.Height * 96 / 72;

            // Remove manual DPI scaling - only use zoom factor
            double renderedPageHeight = pageHeightDip * zoomFactor;

            foreach (var ocrBox in page.OcrBoxes)
            {
                // Convert OCR coordinates to WPF DIPs
                var box = new Rect(
                    ocrBox.Rectangle.X,
                    ocrBox.Rectangle.Y,
                    ocrBox.Rectangle.Width,
                    ocrBox.Rectangle.Height
                );

                var rectangle = new Rectangle
                {
                    Width = box.Width * zoomFactor,
                    Height = box.Height * zoomFactor,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Colors.Blue) { Opacity = 0.2 }
                };

                Canvas.SetLeft(rectangle, box.X * zoomFactor);
                Canvas.SetTop(rectangle, verticalOffset + (box.Y * zoomFactor));

                ToolTipService.SetToolTip(rectangle, ocrBox.Text);
                _overlayCanvas.Children.Add(rectangle);
            }

            verticalOffset += renderedPageHeight + _pageGap;
        }

        _overlayCanvas.Height = verticalOffset;
        ApplyScrollTransform();
    }


    public void HandleScroll(ScrollChangedEventArgs args)
    {
        _horizontalOffset = args.HorizontalOffset;
        _verticalOffset = args.VerticalOffset;
        _viewportWidth = args.ViewportWidth;
        _viewportHeight = args.ViewportHeight;
        ApplyScrollTransform();
    }

    public void zoomChanged()
    {
        RenderAllPages();
    }

    private void ApplyScrollTransform()
    {
        _overlayCanvas.RenderTransform = new TranslateTransform
        {
            X = -_horizontalOffset,
            Y = -_verticalOffset
        };

        _overlayCanvas.Clip = new RectangleGeometry(
            new Rect(_horizontalOffset, _verticalOffset, _viewportWidth, _viewportHeight)
        );
    }

    public void UpdateOverlayOnViewChanged()
    {
        CalculateDocumentHeight();
        RenderAllPages();
    }


    public void Recolor(SearchResult result)
    {
        for (int i = 0; i < _overlayCanvas.Children.Count; i++)
        {
            if (_overlayCanvas.Children[i] is Rectangle rectangle)
            {
                if (i >= result.BoxIndex && i < result.BoxIndex + result.BoxSpan)
                {
                    rectangle.Fill = new SolidColorBrush(Colors.Green) { Opacity = 0.2 };
                    rectangle.Stroke = Brushes.Green;
                }
                else
                {
                    rectangle.Fill = new SolidColorBrush(Colors.Blue) { Opacity = 0.2 };
                    rectangle.Stroke = Brushes.Blue;
                }
            }
        }
    }
    private static T FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
                return result;
            else
            {
                result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
        }
        return null;
    }
}