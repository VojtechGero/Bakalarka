using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
using System;
using System.Collections.Generic;
using System.Windows;

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
}
