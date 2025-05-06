using BakalarkaWpf.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace BakalarkaWpf.Views.UserControls;

/// <summary>
/// Interaction logic for SearchResultNavigation.xaml
/// </summary>
public partial class SearchResultNavigation : UserControl
{
    List<SearchResult> _results = new();
    int shownResult = 1;
    MainPage _mainPage;
    public SearchResultNavigation(List<SearchResult> results, MainPage mainPage)
    {
        _results = results;
        _mainPage = mainPage;
        InitializeComponent();
        FileNameBlock.Text = Path.GetFileName(_results.First().FilePath);
        ResultCount.Text = $"1/{_results.Count}";
    }

    private async void NextButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        shownResult++;
        if (shownResult > _results.Count)
        {
            shownResult = 1;
        }
        ResultCount.Text = $"{shownResult}/{_results.Count}";
    }
}
