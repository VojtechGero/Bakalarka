using BakalarkaWpf.Models;
using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BakalarkaWpf.Views.UserControls
{
    public partial class FolderTreeViewControl : UserControl
    {
        private bool isProgrammaticSelection;
        public event EventHandler<FileItem> TreeFileClicked;
        private string workingFolder;

        public FolderTreeViewControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty RootFolderProperty =
            DependencyProperty.Register("RootFolder", typeof(FileItem), typeof(FolderTreeViewControl),
                new PropertyMetadata(null));

        public FileItem RootFolder
        {
            get => (FileItem)GetValue(RootFolderProperty);
            set => SetValue(RootFolderProperty, value);
        }

        public void Update()
        {
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }
            RootFolder = GetFolderStructure(workingFolder);
            // Wrap root folder in a list to show it as the top node
            TreeView.ItemsSource = new List<FileItem> { RootFolder };
        }


        private FileItem GetFolderStructure(string path)
        {
            var folderName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(folderName)) // Handle root directories
            {
                folderName = path;
            }

            var folder = new FileItem
            {
                Name = folderName,
                Path = path,
                IsDirectory = true,
                SubItems = new List<FileItem>()
            };

            // Add subdirectories
            foreach (var dir in Directory.GetDirectories(path))
            {
                folder.SubItems.Add(GetFolderStructure(dir));
            }

            // Add PDF files
            foreach (var file in Directory.GetFiles(path)
                         .Where(x => x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)))
            {
                folder.SubItems.Add(new FileItem
                {
                    Name = Path.GetFileName(file),
                    Path = file,
                    IsDirectory = false,
                    SubItems = null
                });
            }

            return folder;
        }

        private void TreeView_SelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is FileItem selectedItem && !isProgrammaticSelection)
            {
                TreeFileClicked?.Invoke(this, selectedItem);
            }
            isProgrammaticSelection = false;
        }

        public void SetSelectedItem(FileItem item)
        {
            if (item == null) return;

            var target = FindItemByPath(RootFolder, item.Path);
            if (target == null) return;

            isProgrammaticSelection = true;
            TreeView.SelectedItems.Clear();
            TreeView.SelectedItems.Add(target);
            if (item.IsDirectory)
            {
                ExpandNode(item);
            }
            TreeView.BringIntoView(target);
            isProgrammaticSelection = false;
        }

        private FileItem FindItemByPath(FileItem root, string path)
        {
            if (root.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return root;
            }

            if (root.SubItems != null)
            {
                foreach (var item in root.SubItems)
                {
                    var result = FindItemByPath(item, path);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        public void LoadFolderStructure(string path)
        {
            workingFolder = path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            RootFolder = GetFolderStructure(path);
            TreeView.ItemsSource = new List<FileItem> { RootFolder };
        }

        // Add this method to expand nodes
        public void ExpandNode(FileItem item)
        {
            if (item == null) return;
            var target = FindItemByPath(RootFolder, item.Path);
            var node = FindTreeNode(target);
            if (target != null)
            {
                TreeView.ExpandNode(node);
            }
        }
        public TreeViewNode FindTreeNode(FileItem targetItem)
        {
            if (TreeView.Nodes == null || targetItem == null)
                return null;

            // Search through all root nodes (in our case just the working folder)
            foreach (var rootNode in TreeView.Nodes)
            {
                var node = FindTreeNodeRecursive(rootNode, targetItem);
                if (node != null)
                    return node;
            }
            return null;
        }

        private TreeViewNode FindTreeNodeRecursive(TreeViewNode parentNode, FileItem targetItem)
        {
            // Check if current node matches
            if (parentNode.Content is FileItem item &&
                item.Path.Equals(targetItem.Path, StringComparison.OrdinalIgnoreCase))
            {
                return parentNode;
            }

            // Search child nodes
            if (parentNode.ChildNodes != null)
            {
                foreach (var childNode in parentNode.ChildNodes)
                {
                    var result = FindTreeNodeRecursive(childNode, targetItem);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }
    }
}
