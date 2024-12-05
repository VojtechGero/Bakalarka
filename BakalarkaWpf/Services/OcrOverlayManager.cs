using BakalarkaWpf.Models;
using Syncfusion.Windows.PdfViewer;
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
            Background = Brushes.Transparent
        };

        // Ensure canvas covers the entire PDF view
        _overlayCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
        _overlayCanvas.VerticalAlignment = VerticalAlignment.Stretch;

        // Add canvas to PDF viewer
        var grid = _pdfView.Parent as Grid;
        grid?.Children.Add(_overlayCanvas);

        // Subscribe to zoom and page change events
        _pdfView.ZoomChanged += (s, e) => UpdateOverlayOnViewChanged();
        _pdfView.CurrentPageChanged += (s, e) => RenderOcrOverlay(_pdfView.CurrentPageIndex);
    }

    public void RenderOcrOverlay(int pageNumber)
    {
        // Clear previous overlays
        _overlayCanvas.Children.Clear();

        // Find OCR data for the current page
        var pageOcr = _pdfDocument.Pages.Find(p => p.pageNum == pageNumber);
        if (pageOcr == null) return;

        // Calculate scaling factors
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

            // Calculate precise positioning
            Canvas.SetLeft(rectangle, ocrBox.Rectangle.X * zoomFactor);
            Canvas.SetTop(rectangle, ocrBox.Rectangle.Y * zoomFactor);

            // Add tooltip with OCR text
            ToolTipService.SetToolTip(rectangle, ocrBox.Text);
            _overlayCanvas.Children.Add(rectangle);
        }
    }

    public void UpdateOverlayOnViewChanged()
    {
        // Rerender overlay for current page
        RenderOcrOverlay(_pdfView.CurrentPageIndex);
    }
}