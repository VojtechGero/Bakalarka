﻿using BakalarkaWpf.Models;
using BakalarkaWpf.ViewModels;
using BakalarkaWpf.Views.UserControls;
using System;
using System.IO;
using Page = System.Windows.Controls.Page;

namespace BakalarkaWpf.Views;

public partial class MainPage : Page
{
    private DMS _dms;
    private PdfDisplay? _PdfDisplay = null;
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _dms = new DMS();
        _dms.FileSelected += Dms_FileSelected;
        _dms.SearchSelected += SearchSelected;
        mainGrid.Children.Add(_dms);
    }

    private async void SearchSelected(object? sender, FileResults results)
    {
        if (_PdfDisplay is not null && _PdfDisplay?._fileItem.Path == results.FilePath)
        {
            mainGrid.Children.Clear();
            mainGrid.Children.Add(_PdfDisplay);
        }
        else
        {
            _PdfDisplay = new PdfDisplay(new FileItem()
            {
                Path = results.FilePath,
                Name = Path.GetFileName(results.FilePath),
                IsDirectory = false
            });
            mainGrid.Children.Clear();
            mainGrid.Children.Add(_PdfDisplay);
            _PdfDisplay.BackButtonPressed += BackButtonPressed;
        }
        _PdfDisplay.openSearchBox(results);
    }

    private void Dms_FileSelected(object? sender, FileItem item)
    {
        if (_PdfDisplay is not null && _PdfDisplay?._fileItem.Path == item.Path)
        {
            mainGrid.Children.Clear();
            mainGrid.Children.Add(_PdfDisplay);
        }
        else
        {
            _PdfDisplay = new PdfDisplay(item);
            mainGrid.Children.Clear();
            mainGrid.Children.Add(_PdfDisplay);
            _PdfDisplay.BackButtonPressed += BackButtonPressed;
        }

    }

    private void BackButtonPressed(object? sender, EventArgs e)
    {
        mainGrid.Children.Clear();
        mainGrid.Children.Add(_dms);
        _dms.UpdateFolderTreeView();
    }
}
