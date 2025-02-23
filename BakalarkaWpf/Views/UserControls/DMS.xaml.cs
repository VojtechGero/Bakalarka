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
        public event EventHandler<FileItem> ListFileClicked;
        public DMS()
        {
            InitializeComponent();
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
    }
}
