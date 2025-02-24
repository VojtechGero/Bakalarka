using BakalarkaWpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace BakalarkaWpf.Views.UserControls
{
    /// <summary>
    /// Interaction logic for DMS.xaml
    /// </summary>
    public partial class DMS : UserControl
    {
        List<FileItem> _fileItems;
        private string _workingFolder;
        public event EventHandler<FileItem> FileSelected;
        public event EventHandler<FileResults> SearchSelected;
        SearchWindow _searchWindow = null;
        public DMS(string workingFolder)
        {
            _workingFolder = workingFolder;
            InitializeComponent();
            FolderTreeControl.LoadFolderStructure(_workingFolder);
            UpdateItems(_workingFolder);
            FolderTreeControl.TreeFileClicked += TreeFileClicked;
        }
        public void UpdateItems(string path)
        {
            _fileItems = LoadTopLevelFolderItems(path);
            FilesPanel.Children.Clear();
            foreach (FileItem item in _fileItems)
            {
                FileDisplay file = new FileDisplay(item);
                file.ListFileClicked += ListFileClicked;
                FilesPanel.Children.Add(file);
            }
        }
        private void TreeFileClicked(object sender, FileItem fileItem)
        {
            if (fileItem.IsDirectory)
            {
                UpdateItems(fileItem.Path);
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
            }
            else
            {
                FileSelected?.Invoke(this, fileItem);
            }
            //FolderTreeControl.SetSelectedItem(fileItem);
        }

        private List<FileItem> LoadTopLevelFolderItems(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var items = new List<FileItem>();

            foreach (var dir in Directory.GetDirectories(path))
            {
                items.Add(new FileItem
                {
                    Name = Path.GetFileName(dir),
                    Path = dir,
                    IsDirectory = true,
                    SubItems = null
                });
            }
            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    continue;

                items.Add(new FileItem
                {
                    Name = Path.GetFileName(file),
                    Path = file,
                    IsDirectory = false,
                    SubItems = null
                });
            }

            return items;
        }

        private void SearchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _searchWindow = new SearchWindow();
            _searchWindow.Show();
            _searchWindow.SearchSelected += SearchSelected;
        }

        private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
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
            string newPath = Path.Combine(_workingFolder, name);
            File.Copy(filePath, newPath);
            FolderTreeControl.Update();
            return newPath;
        }
    }
}
