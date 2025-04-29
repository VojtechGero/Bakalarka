using BakalarkaWpf.Models;
using BakalarkaWpf.Services;
using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BakalarkaWpf.Views.UserControls
{
    public partial class FolderTreeViewControl : UserControl
    {
        private bool isProgrammaticSelection;
        public event EventHandler<FileItem> TreeFileClicked;
        private string workingFolder;
        private readonly ApiFileService _fileService;

        public FolderTreeViewControl()
        {
            InitializeComponent();
            _fileService = new ApiFileService();
        }

        public void InitTree()
        {
            TreeView.SelectAll();
            SetSelectedItem(RootFolder);
        }
        public static readonly DependencyProperty RootFolderProperty =
            DependencyProperty.Register("RootFolder", typeof(FileItem), typeof(FolderTreeViewControl),
                new PropertyMetadata(null));

        public FileItem RootFolder
        {
            get => (FileItem)GetValue(RootFolderProperty);
            set => SetValue(RootFolderProperty, value);
        }

        public async Task Update()
        {
            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }
            RootFolder = await GetFolderStructure();
            workingFolder = RootFolder.Path;
            TreeView.ItemsSource = new List<FileItem> { RootFolder };
        }


        private async Task<FileItem> GetFolderStructure()
        {
            return await _fileService.GetTopAllItems();
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

            ExpandParentNodes(target);

            if (target.IsDirectory)
            {
                ExpandNode(target);
            }
            TreeView.BringIntoView(target);
            isProgrammaticSelection = false;
        }

        private void ExpandParentNodes(FileItem item)
        {
            var node = FindTreeNode(item);
            if (node == null) return;

            var parentNode = node.ParentNode;
            while (parentNode != null)
            {
                TreeView.ExpandNode(parentNode);
                parentNode = parentNode.ParentNode;
            }
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

        public async Task<string> LoadFolderStructure()
        {
            RootFolder = await GetFolderStructure();
            TreeView.ItemsSource = new List<FileItem> { RootFolder };
            TreeView.SelectAll();
            SetSelectedItem(RootFolder);
            workingFolder = RootFolder.Path;
            return workingFolder;
        }

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
            if (parentNode.Content is FileItem item &&
                item.Path.Equals(targetItem.Path, StringComparison.OrdinalIgnoreCase))
            {
                return parentNode;
            }
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
