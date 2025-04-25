using System.Windows.Controls;

namespace BakalarkaWpf.Views.UserControls;

/// <summary>
/// Interaction logic for MyProgressBar.xaml
/// </summary>
public partial class MyProgressBar : UserControl
{
    public string filePath { get; set; }
    public MyProgressBar(string filePath)
    {
        InitializeComponent();
        this.filePath = filePath;
        nameLabel.Content = $"Získávání přepisu dokumentu: {filePath}";
    }

}
