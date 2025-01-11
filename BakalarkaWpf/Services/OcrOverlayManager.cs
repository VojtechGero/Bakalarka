using BakalarkaWpf.Models;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Windows.PdfViewer;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BakalarkaWpf.Services;

public class OcrOverlayManager
{
    private readonly Dictionary<int, Canvas> _pageOverlays;
    private readonly PdfDocumentView _pdfView;
    private readonly PdfLoadedDocument _document;
    private readonly Pdf _pdfDocument;
    private readonly Grid _parentGrid;
    private double _currentZoom = 1.0;
    private double _verticalOffset = 0;
    private double _horizontalOffset = 0;
    private double _viewportHeight = 0;
    private double _viewportWidth = 0;
    private const double PAGE_SPACING = 20.0;
    private const double VIEWPORT_BUFFER = 500.0;

    public OcrOverlayManager(PdfDocumentView pdfView, Pdf pdfDocument, PdfLoadedDocument document)
    {
        _pdfView = pdfView;
        _pdfDocument = pdfDocument;
        _document = document;
        _pageOverlays = new Dictionary<int, Canvas>();
        _parentGrid = _pdfView.Parent as Grid;

        InitializeOverlays();
        SetupEventHandlers();
    }
    private void InitializeOverlays()
    {
        // Clear existing overlays
        if (_parentGrid != null)
        {
            var existingOverlays = _parentGrid.Children.OfType<Canvas>()
                .Where(c => c.Background == Brushes.Transparent)
                .ToList();
            foreach (var overlay in existingOverlays)
            {
                _parentGrid.Children.Remove(overlay);
            }
        }

        // Initialize overlays for all pages
        for (int i = 0; i < _pdfDocument.Pages.Count; i++)
        {
            CreateOverlayForPage(i);
        }
    }
    private void SetupEventHandlers()
    {
        _pdfView.ZoomChanged += (s, e) =>
        {
            _currentZoom = _pdfView.ZoomPercentage / 100.0;
            UpdateAllOverlays();
        };

        // Subscribe to size changes of the PDF viewer
        _pdfView.SizeChanged += (s, e) =>
        {
            _viewportWidth = _pdfView.ActualWidth;
            _viewportHeight = _pdfView.ActualHeight;
            UpdateOverlayPositions();
        };
    }

    private void CreateOverlayForPage(int pageNumber)
    {
        var overlay = new Canvas
        {
            IsHitTestVisible = false,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        _pageOverlays[pageNumber] = overlay;
        _parentGrid?.Children.Add(overlay);

        // Set initial Z-index to ensure overlay is above PDF
        Panel.SetZIndex(overlay, 100 + pageNumber);

        RenderOcrOverlay(pageNumber);
    }

    public void HandleScroll(ScrollChangedEventArgs args)
    {
        _verticalOffset = args.VerticalOffset;
        _horizontalOffset = args.HorizontalOffset;
        _viewportHeight = args.ViewportHeight;
        _viewportWidth = args.ViewportWidth;

        UpdateOverlayPositions();
    }

    private void UpdateOverlayPositions()
    {
        double currentTop = 0;
        var pageSize = GetPageSize(0); // Get size of first page as reference

        foreach (var pageNumber in _pageOverlays.Keys)
        {
            var overlay = _pageOverlays[pageNumber];

            // Calculate position considering zoom and scroll
            double pageTop = (currentTop * _currentZoom) - _verticalOffset;
            double pageLeft = -_horizontalOffset;

            // Update overlay position and size
            overlay.Width = pageSize.Width * _currentZoom;
            overlay.Height = pageSize.Height * _currentZoom;

            Canvas.SetLeft(overlay, pageLeft);
            Canvas.SetTop(overlay, pageTop);

            // Show/hide based on viewport visibility
            overlay.Visibility = IsPageVisible(pageTop, pageSize.Height * _currentZoom)
                ? Visibility.Visible
                : Visibility.Collapsed;

            currentTop += pageSize.Height + PAGE_SPACING;
        }
    }
    private Size GetPageSize(int pageNumber)
    {
        var page = _document.Pages[pageNumber];
        return new Size(page.Size.Width, page.Size.Height);
    }

    private bool IsPageVisible(double pageTop, double pageHeight)
    {
        return (pageTop + pageHeight >= -VIEWPORT_BUFFER) &&
               (pageTop <= _viewportHeight + VIEWPORT_BUFFER);
    }


    public void hideOverlay()
    {
        foreach (var overlay in _pageOverlays.Values)
        {
            overlay.Visibility = Visibility.Hidden;
        }
    }

    public void showOverlay()
    {
        foreach (var overlay in _pageOverlays.Values)
        {
            overlay.Visibility = Visibility.Visible;
        }
        UpdateOverlayPositions();
    }

    private void UpdateAllOverlays()
    {
        foreach (var pageNumber in _pageOverlays.Keys)
        {
            RenderOcrOverlay(pageNumber);
        }
        UpdateOverlayPositions();
    }

    public void RenderOcrOverlay(int pageNumber)
    {
        if (!_pageOverlays.TryGetValue(pageNumber, out var overlay)) return;

        overlay.Children.Clear();
        var pageOcr = _pdfDocument.Pages.FirstOrDefault(p => p.pageNum == pageNumber);
        if (pageOcr == null) return;

        var pageSize = GetPageSize(pageNumber);
        var scaleX = _currentZoom;
        var scaleY = _currentZoom;

        foreach (var ocrBox in pageOcr.OcrBoxes)
        {
            var rectangle = new Rectangle
            {
                Width = ocrBox.Rectangle.Width * scaleX,
                Height = ocrBox.Rectangle.Height * scaleY,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Colors.Blue) { Opacity = 0.2 }
            };

            Canvas.SetLeft(rectangle, ocrBox.Rectangle.X * scaleX);
            Canvas.SetTop(rectangle, ocrBox.Rectangle.Y * scaleY);

            ToolTipService.SetToolTip(rectangle, ocrBox.Text);
            overlay.Children.Add(rectangle);
        }
    }

    public void Recolor(SearchResult result)
    {
        if (!_pageOverlays.TryGetValue(_pdfView.CurrentPageIndex, out var overlay)) return;

        for (int i = 0; i < overlay.Children.Count; i++)
        {
            if (overlay.Children[i] is Rectangle rectangle)
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