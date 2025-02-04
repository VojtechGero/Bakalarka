using BakalarkaWpf.Models;
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
    private readonly PdfDocumentView _pdfView;
    private readonly Pdf _pdfDocument;
    public OcrOverlayManager(PdfDocumentView pdfView, Pdf pdfDocument)
    {
        _pdfView = pdfView;
        _pdfDocument = pdfDocument;
        _overlayCanvas = new Canvas
        {
            IsHitTestVisible = false,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        var grid = _pdfView.Parent as Grid; //CenterSegment
        if (grid != null)
        {
            var existingOverlay = grid.Children.OfType<Canvas>()
                .FirstOrDefault(c => c.Background == Brushes.Transparent);
            if (existingOverlay != null)
            {
                grid.Children.Remove(existingOverlay);
            }

            grid.Children.Add(_overlayCanvas);
        }

        _pdfView.CurrentPageChanged += (s, e) => RenderOcrOverlay(_pdfView.CurrentPageIndex);
    }

    public void hideOverlay()
    {
        if (_overlayCanvas != null)
        {
            _overlayCanvas.Visibility = Visibility.Hidden;
        }
    }
    public void showOverlay()
    {
        if (_overlayCanvas != null)
        {
            _overlayCanvas.Visibility = Visibility.Visible;
            //RenderOcrOverlay(_pdfView.CurrentPageIndex); //not sure if needed...
        }
    }
    public void RenderOcrOverlay(int pageNumber)
    {
        _overlayCanvas.Children.Clear();

        var pageOcr = _pdfDocument.Pages.Find(p => p.pageNum == pageNumber);
        if (pageOcr == null) return;

        double zoomFactor = _pdfView.ZoomPercentage / 100.0;

        foreach (var ocrBox in pageOcr.OcrBoxes)
        {
            var rectangle = new Rectangle
            {
                Width = ocrBox.Rectangle.Width * zoomFactor,
                Height = ocrBox.Rectangle.Height * zoomFactor,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Colors.Blue) { Opacity = 0.2 }
            };

            Canvas.SetLeft(rectangle, ocrBox.Rectangle.X * zoomFactor);
            Canvas.SetTop(rectangle, ocrBox.Rectangle.Y * zoomFactor);

            ToolTipService.SetToolTip(rectangle, ocrBox.Text);
            _overlayCanvas.Children.Add(rectangle);
        }
    }

    public void UpdateOverlayOnViewChanged()
    {
        RenderOcrOverlay(_pdfView.CurrentPageIndex);
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
}