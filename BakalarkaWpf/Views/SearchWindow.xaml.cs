using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BakalarkaWpf.Views;

/// <summary>
/// Interaction logic for SearchWindow.xaml
/// </summary>
public partial class SearchWindow : Window
{

    private ApiSearchService _searchService;
    private List<FileResults> _results;
    public EventHandler<FileResults> SearchSelected;
    public SearchWindow()
    {
        InitializeComponent();
        _searchService = new ApiSearchService();
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await search();
    }

    private async Task search()
    {
        if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
        {
            _results = await _searchService.SearchAsync(SearchTextBox.Text);
            ResultsListBox.ItemsSource = _results;
        }
    }
    private void ResultsListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ResultsListBox.SelectedItem is FileResults selectedItem)
        {
            SearchSelected?.Invoke(this, selectedItem);
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_results == null || _results.Count == 0)
        {
            MessageBox.Show("No search results to export.", "Export Error",
                          MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = "SearchResults",
            DefaultExt = ".xlsx",
            Filter = "Excel Files (.xlsx)|*.xlsx",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                var exportService = new ExcelExportService();
                exportService.ExportToExcel(_results, saveFileDialog.FileName);

                MessageBox.Show("Výsledky úspěšně exportovány!", "Export dokončen",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Výsledky nebylo možné exportovat", "Export Error",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {

            await search();
        }
    }
}
