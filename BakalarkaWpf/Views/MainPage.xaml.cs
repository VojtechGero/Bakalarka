using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
using BakalarkaWpf.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Page = System.Windows.Controls.Page;

namespace BakalarkaWpf.Views;

public partial class MainPage : Page
{
    private string workingForlder;
    private string loadedFile = "";
    private string query = "";
    private OcrOverlayManager _ocrOverlayManager;
    private ApiSearchService _searchService;
    private bool isSearchPanelVisible = false;
    private bool DocumentLoaded = false;
    private bool overlayShowed = false;
    private List<FileResults> searchResults = new List<FileResults>();
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        workingForlder = Path.GetFullPath("./data");

        DataContext = viewModel;
        _searchService = new ApiSearchService();
        FolderTreeControl.LoadFolderStructure(workingForlder);
        Dms.UpdateItems(workingForlder);
        FolderTreeControl.TreeFileClicked += TreeFileClicked;
        Dms.ListFileClicked += ListFileClicked;
    }
    private void TreeFileClicked(object sender, FileItem fileItem)
    {
        if (fileItem.IsDirectory)
        {
            Dms.UpdateItems(fileItem.Path);
        }
    }
    private void ListFileClicked(object sender, FileItem fileItem)
    {
        if (fileItem.IsDirectory)
        {
            Dms.UpdateItems(fileItem.Path);
        }
        FolderTreeControl.SetSelectedItem(fileItem);
    }

    private async void LoadButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            FileName = "Document",
            DefaultExt = ".pdf",
            Filter = "Pdf document (.pdf)|*.pdf"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            string localPath = copyFile(dialog.FileName);


        }
    }

    private string copyFile(string filePath)
    {
        string name = Path.GetFileName(filePath);
        string newPath = Path.Combine(workingForlder, name);
        File.Copy(filePath, newPath);
        FolderTreeControl.Update();
        return newPath;
    }




    private void HighlightResult(object sender, MouseEventArgs e)
    {
        TextBlock block = (TextBlock)sender;
        block.Background = System.Windows.Media.Brushes.Aqua;
    }
    private void RemoveHighlight(object sender, MouseEventArgs e)
    {
        TextBlock block = (TextBlock)sender;
        block.Background = System.Windows.Media.Brushes.White;
    }

    private void PDFView_DocumentLoaded(object sender, EventArgs args)
    {
        DocumentLoaded = true;
    }

    private void ShowButton_Click(object sender, RoutedEventArgs e)
    {
        if (overlayShowed)
        {
            _ocrOverlayManager.HideOverlay();
        }
        else
        {
            _ocrOverlayManager.ShowOverlay();
        }
        overlayShowed = !overlayShowed;
    }

    private void PDFView_ScrollChanged(object sender, ScrollChangedEventArgs args)
    {
        _ocrOverlayManager?.HandleScroll(args);
    }


}
