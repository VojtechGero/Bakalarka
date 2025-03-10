using System.Windows;

namespace BakalarkaWpf.Views
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public string ResponseText { get; set; }

        public InputDialog(string prompt, string defaultResponse = "")
        {
            InitializeComponent();
            PromptText.Text = prompt;
            ResponseTextBox.Text = defaultResponse;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            ResponseText = ResponseTextBox.Text;
        }
    }
}
