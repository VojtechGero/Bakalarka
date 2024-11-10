using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BakalarkaWpf.Views.UserControls;

/// <summary>
/// Interaction logic for FolderTreeViewControl.xaml
/// </summary>
public partial class FolderTreeViewControl : UserControl
{
    public event EventHandler<string> PdfFileClicked;
    public FolderTreeViewControl()
    {
        InitializeComponent();
    }

    // RootFolder as a dependency property
    public static readonly DependencyProperty RootFolderProperty =
        DependencyProperty.Register("RootFolder", typeof(FolderItem), typeof(FolderTreeViewControl),
            new PropertyMetadata(null));

    public FolderItem RootFolder
    {
        get { return (FolderItem)GetValue(RootFolderProperty); }
        set { SetValue(RootFolderProperty, value); }
    }

    // Method to load the folder structure
    public void LoadFolderStructure(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        RootFolder = GetFolderStructure(path);
        TreeView.ItemsSource = RootFolder.Items; // Set the TreeView's ItemsSource
    }

    private FolderItem GetFolderStructure(string path)
    {
        var folder = new FolderItem
        {
            Name = Path.GetFileName(path),
            Path = path
        };

        // Add subfolders to the Items collection
        foreach (var dir in Directory.GetDirectories(path))
        {
            var subFolder = GetFolderStructure(dir);
            folder.SubFolders.Add(subFolder);       // Add to SubFolders for hierarchy
            folder.Items.Add(subFolder);            // Add to Items for TreeView display
        }

        // Add files to the Items collection
        foreach (var file in Directory.GetFiles(path))
        {
            var fileItem = new FileItem
            {
                Name = Path.GetFileName(file),
                Path = file
            };
            folder.Items.Add(fileItem);
        }

        return folder;
    }
    private void TreeView_SelectionChanged(object sender, Syncfusion.UI.Xaml.TreeView.ItemSelectionChangedEventArgs e)
    {
        // Get the selected item from the TreeView
        var selectedItem = e.AddedItems[0] as FileItem;
        if (selectedItem != null)
        {
            // Check if the selected item is a PDF
            if (selectedItem.Path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                // Raise the event and pass the file path
                PdfFileClicked?.Invoke(this, selectedItem.Path);
            }
        }
    }
}



// Define the data models
public class FileItem
{
    public string Name { get; set; }
    public string Path { get; set; }
}

public class FolderItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    public ObservableCollection<FolderItem> SubFolders { get; set; } = new ObservableCollection<FolderItem>();
    public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();
}
