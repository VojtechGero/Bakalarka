using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
using Syncfusion.Pdf.Parsing;
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BakalarkaWpf.Views.UserControls;

/// <summary>
/// Interaction logic for DMS.xaml
/// </summary>
public partial class DMS : UserControl
{
    List<FileItem> _fileItems;
    FileItem _currentHead;
    private string _workingFolder;
    private readonly ApiFileService _fileService;
    public event EventHandler<FileItem> FileSelected;
    public event EventHandler<FileResults> SearchSelected;
    SearchWindow _searchWindow = null;
    private List<string> _clipboardPaths = new List<string>();
    private bool _isCopyOperation = false;
    public DMS()
    {
        _fileService = new ApiFileService();

        InitializeComponent();
        LoadFiles();
        FolderTreeControl.InitTree();

    }
    public void UpdateFolderTreeView()
    {

        FolderTreeControl.SetSelectedItem(_currentHead);
    }
    private async Task LoadFiles()
    {
        _workingFolder = await FolderTreeControl.LoadFolderStructure();
        _currentHead = new FileItem()
        {
            Name = Path.GetFileName(_workingFolder),
            Path = _workingFolder,
            IsDirectory = true,
            SubItems = null
        };
        await UpdateItems(_workingFolder);
        FolderTreeControl.TreeFileClicked += TreeFileClicked;
    }

    public async Task UpdateItems(string path)
    {
        _fileItems = await LoadTopLevelFolderItems(path);
        FilesTreeGrid.ItemsSource = _fileItems;
        FilesTreeGrid.ColumnSizer = TreeColumnSizer.Auto;
        this.InvalidateVisual();
        this.UpdateLayout();
        FilesTreeGrid.ColumnSizer = TreeColumnSizer.SizeToCells;

    }
    private async void TreeFileClicked(object sender, FileItem fileItem)
    {
        if (fileItem.IsDirectory)
        {
            await UpdateItems(fileItem.Path);
            _currentHead = fileItem;
        }
        else
        {
            FileSelected?.Invoke(this, fileItem);
        }
    }

    private async Task<List<FileItem>> LoadTopLevelFolderItems(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var items = await _fileService.GetTopLevelItems(path);
        return items;
    }

    private void SearchButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _searchWindow = new SearchWindow();
        _searchWindow.Show();
        _searchWindow.SearchSelected += SearchSelected;
    }

    private async void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            FileName = "Dokument",
            DefaultExt = ".pdf",
            Filter = "Pdf dokument (.pdf)|*.pdf",
            Multiselect = true
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            var progressBar = new MyProgressBar()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            CenterSegment.Children.Add(progressBar);
            bool updated = false;
            foreach (var file in dialog.FileNames)
            {
                progressBar.UpdateMessage($"Získávání přepisu dokumentu: {Path.GetFileName(file)}");
                bool uploadResult = await _fileService.UploadFileAsync(file, _currentHead.Path);
                updated = updated || uploadResult;
                if (uploadResult == false)
                {
                    MessageBox.Show($"Nahrávání dokumentu {Path.GetFileName(file)} selhalo");
                    continue;
                }
                try
                {
                    PdfLoadedDocument doc = new PdfLoadedDocument(file);
                    await _fileService.GetOcrAsync(Path.Combine(_currentHead.Path, Path.GetFileName(file)), (int)doc.Pages[0].Size.Height, (int)doc.Pages[0].Size.Width);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Clipboard.SetText(ex.Message);
                }

            }
            if (updated)
            {

                await UpdateItems(_currentHead.Path);
                await FolderTreeControl.Update();
                FolderTreeControl.SetSelectedItem(_currentHead);
            }
            CenterSegment.Children.Remove(progressBar);
        }
    }

    private void FilesTreeGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var treeGrid = sender as SfTreeGrid;
        var selectedItem = treeGrid.SelectedItem as FileItem;

        if (selectedItem != null)
        {
            if (selectedItem.IsDirectory)
            {
                _currentHead = selectedItem;
                UpdateItems(selectedItem.Path);
                FolderTreeControl.SetSelectedItem(selectedItem);
            }
            else
            {
                FileSelected?.Invoke(this, selectedItem);
            }
        }
    }

    private async void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var selectedItem = FilesTreeGrid.SelectedItem as FileItem;
        if (selectedItem == null) return;

        var dialog = new InputDialog("Nové jméno:", selectedItem.Name);
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ResponseText))
        {
            bool success = await _fileService.RenameItemAsync(
                selectedItem.Path,
                dialog.ResponseText
            );

            if (success)
            {
                await UpdateItems(_currentHead.Path);
                await FolderTreeControl.Update();
                FolderTreeControl.SetSelectedItem(_currentHead);
            }
            else
            {
                MessageBox.Show("Přejmenování selhalo.");
            }
        }
    }

    private async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = FilesTreeGrid.SelectedItems.Cast<FileItem>().ToList();
        if (selectedItems.Count == 0) return;

        var result = MessageBox.Show(
            $"Chcete smazat položku?",
            "Potvrdit smazání",
            MessageBoxButton.YesNo
        );

        if (result != MessageBoxResult.Yes) return;

        bool allSuccess = true;
        foreach (var item in selectedItems)
        {
            bool success = await _fileService.DeleteItemAsync(item.Path);
            if (!success) allSuccess = false;
        }

        if (allSuccess)
        {
            await UpdateItems(_currentHead.Path);
            await FolderTreeControl.Update();
            FolderTreeControl.SetSelectedItem(_currentHead);
        }
        else
        {
            MessageBox.Show("Some items couldn't be deleted.");
        }
    }

    private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _clipboardPaths = FilesTreeGrid.SelectedItems
            .Cast<FileItem>()
            .Select(item => item.Path)
            .ToList();
        _isCopyOperation = true;
    }

    private void MoveMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _clipboardPaths = FilesTreeGrid.SelectedItems
            .Cast<FileItem>()
            .Select(item => item.Path)
            .ToList();
        _isCopyOperation = false;
    }

    private async void NewFolderMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new InputDialog("Folder name:", "New Folder");
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.ResponseText))
        {
            string newPath = Path.Combine(_currentHead.Path, dialog.ResponseText);
            bool success = await _fileService.CreateFolderAsync(newPath);

            if (success)
            {
                await UpdateItems(_currentHead.Path);
                FolderTreeControl.Update();
            }
            else
            {
                MessageBox.Show("Folder creation failed.");
            }
        }
    }

    private async void PasteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_clipboardPaths.Count == 0 || _currentHead == null) return;

        bool allSuccess = true;
        foreach (var sourcePath in _clipboardPaths)
        {
            bool success = _isCopyOperation
                ? await _fileService.CopyItemAsync(sourcePath, _currentHead.Path)
                : await _fileService.MoveItemAsync(sourcePath, _currentHead.Path);

            if (!success) allSuccess = false;
        }

        if (allSuccess)
        {
            await UpdateItems(_currentHead.Path);
            await FolderTreeControl.Update();
            FolderTreeControl.SetSelectedItem(_currentHead);
            _clipboardPaths.Clear();
        }
        else
        {
            MessageBox.Show("Vkládání selhalo.");
        }
    }


    private void FilesTreeGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {

        PasteMenuItem.IsEnabled = _clipboardPaths.Count > 0;
    }
}

