using BakalarkaWpf.ViewModels;
using System.Windows.Controls;

namespace BakalarkaWpf.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void LoadButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.FileName = "Document";
        dialog.DefaultExt = ".pdf";
        dialog.Filter = "Pdf document (.pdf)|*.pdf";
        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            string filename = dialog.FileName;
            PDFView.Load(filename);
            FileMessage.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
