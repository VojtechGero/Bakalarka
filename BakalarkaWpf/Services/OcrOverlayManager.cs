using BakalarkaWpf.Models;
using Syncfusion.Data.Extensions;
using Syncfusion.Pdf.Parsing;
using Syncfusion.UI.Xaml.Diagram;
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
        RenderAllPages();
    }



    public void HideOverlay() => _overlayCanvas.Visibility = Visibility.Hidden;

    public void ShowOverlay()
    {
        _overlayCanvas.Visibility = Visibility.Visible;
        RenderAllPages();
    }

    public double GetActualZoomFactor()
    {
        if (_loadedDocument.Pages.Count == 0) return 1.0;
        var veiw = FindChild<PdfDocumentView>(_pdfView);
        var scrollViewer = FindChild<ScrollViewer>(veiw);
        if (scrollViewer == null) return _pdfView.ZoomPercentage;
        var dpi = VisualTreeHelper.GetDpi(_pdfView);
        double pageWidthPoints = _loadedDocument.Pages[0].Size.Width;
        double pageWidthWpf = pageWidthPoints * (96.0 / 72.0) * dpi.DpiScaleY;
        return scrollViewer.ViewportWidth / pageWidthWpf;
    }

    private void RenderAllPages()
    {
        _overlayCanvas.Children.Clear();
        double zoomFactor = GetActualZoomFactor();
        double verticalOffset = 0;
        double dpiScale = 1.0;
        var source = PresentationSource.FromVisual(_overlayCanvas);
        if (source != null)
        {
            dpiScale = source.CompositionTarget.TransformToDevice.M11;
        }
        dpiScale = dpiScale * 1.3333;
        foreach (var page in _pdfDocument.Pages.OrderBy(p => p.pageNum))
        {
            var pageSize = _loadedDocument.Pages[page.pageNum - 1].Size;
            double pageHeight = pageSize.Height * dpiScale * zoomFactor;

            foreach (var ocrBox in page.OcrBoxes)
            {
                var rectangle = new Rectangle
                {
                    Width = ocrBox.Width * zoomFactor,
                    Height = ocrBox.Height * zoomFactor,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Colors.Blue) { Opacity = 0.2 }
                };

                Canvas.SetLeft(rectangle, ocrBox.X * zoomFactor);
                Canvas.SetTop(rectangle, verticalOffset + (ocrBox.Y * zoomFactor));

                ToolTipService.SetToolTip(rectangle, ocrBox.Text);
                _overlayCanvas.Children.Add(rectangle);
            }
            verticalOffset += pageHeight + (_pageGap * dpiScale);
        }
        _overlayCanvas.Height = verticalOffset;
        ApplyScrollTransform();
    }

    private static T FindChild<T>(DependencyObject parent, string name = null) where T : DependencyObject
    {
        if (parent == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result && (name == null || ((FrameworkElement)child).Name == name))
                return result;

            var nestedResult = FindChild<T>(child, name);
            if (nestedResult != null)
                return nestedResult;
        }
        return null;
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


    public double GetBoxVerticalOffset(SearchResult result)
    {
        var rectangle = _overlayCanvas.Children.OfType<Rectangle>().ElementAt(result.BoxIndex);

        return Canvas.GetTop(rectangle);
    }
    public void Recolor(SearchResult result)
    {
        var rectangles = _overlayCanvas.Children.OfType<Rectangle>().ToList();

        for (int i = 0; i < rectangles.Count; i++)
        {
            if (i == result.BoxIndex)
            {
                rectangles[i].Fill = new SolidColorBrush(Colors.Green) { Opacity = 0.2 };
                rectangles[i].Stroke = Brushes.Green;
            }
            else
            {
                rectangles[i].Fill = new SolidColorBrush(Colors.Blue) { Opacity = 0.2 };
                rectangles[i].Stroke = Brushes.Blue;
            }
        }
    }
    public void ClearColors()
    {
        _overlayCanvas.Children.OfType<Rectangle>().ForEach(x =>
        {
            x.Stroke = Brushes.Blue;
            x.Fill = new SolidColorBrush(Colors.Blue) { Opacity = 0.2 };
        });
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