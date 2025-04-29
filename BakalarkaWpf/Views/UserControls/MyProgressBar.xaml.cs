using System.Windows.Controls;

namespace BakalarkaWpf.Views.UserControls;

/// <summary>
/// Interaction logic for MyProgressBar.xaml
/// </summary>
public partial class MyProgressBar : UserControl
{
    public string message = "";
    public MyProgressBar()
    {
        InitializeComponent();
    }
    public void UpdateMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            this.message = message;
        }
        nameLabel.Content = this.message;
    }

}
