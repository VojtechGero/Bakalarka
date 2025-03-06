using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BakalarkaWpf.Views.UserControls
{
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
        public DMS()
        {
            _fileService = new ApiFileService();

            InitializeComponent();
            LoadFiles();

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
            FilesTreeGrid.ItemsSource = _fileItems; // Bind to TreeGrid
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
        private void ListFileClicked(object sender, FileItem fileItem)
        {
            if (fileItem.IsDirectory)
            {
                UpdateItems(fileItem.Path);
                FolderTreeControl.SetSelectedItem(fileItem);
                _currentHead = fileItem;
            }
            else
            {
                FileSelected?.Invoke(this, fileItem);
            }
            //FolderTreeControl.SetSelectedItem(fileItem);
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
                FileName = "Document",
                DefaultExt = ".pdf",
                Filter = "Pdf document (.pdf)|*.pdf",
                Multiselect = true
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                bool updated = false;
                foreach (var file in dialog.FileNames)
                {
                    bool uploadResult = await _fileService.UploadFileAsync(file, _currentHead.Path);
                    updated = updated || uploadResult;
                    if (uploadResult == false)
                    {
                        MessageBox.Show("File upload failed");
                    }
                }
                if (updated)
                {
                    FolderTreeControl.Update();
                    FolderTreeControl.SetSelectedItem(_currentHead);
                }

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
                    // Navigate into directory
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
    }
}

