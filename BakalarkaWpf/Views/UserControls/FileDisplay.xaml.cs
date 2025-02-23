using BakalarkaWpf.Models;
using System;
using System.Windows.Controls;
namespace BakalarkaWpf.Views.UserControls;

/// <summary>
/// Interaction logic for FileDisplay.xaml
/// </summary>
public partial class FileDisplay : UserControl
{
    FileItem _fileItem;
    public event EventHandler<FileItem> ListFileClicked;
    public FileDisplay(FileItem fileItem)
    {
        InitializeComponent();
        _fileItem = fileItem;
        FileName.Content = _fileItem.Name;
        FileIcon.Content = _fileItem.IsDirectory ? "FolderIcon" : "PDFIcon";
    }

    private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        innerElement.Background = System.Windows.Media.Brushes.LightBlue;
    }

    private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        innerElement.Background = System.Windows.Media.Brushes.White;
    }

    private void UserControl_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        ListFileClicked?.Invoke(this, _fileItem);
    }
}
