// InputDialog.xaml.cs
using System.Windows;

namespace Kesmai.WorldForge.UI.Documents
{
    public partial class InputDialog : Window
    {
        public string Input { get; private set; }

        public InputDialog(string title, string defaultInput)
        {
            InitializeComponent();
            Title = title;
            InputTextBox.Text = defaultInput;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Input = InputTextBox.Text;
            DialogResult = true;
        }
    }
}