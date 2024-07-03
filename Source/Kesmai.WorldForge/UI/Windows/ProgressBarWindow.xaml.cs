namespace Kesmai.WorldForge.UI.Windows
{
    using System;
    using System.Windows;

    public partial class ProgressBarWindow : Window
    {
        public ProgressBarWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int value)
        {
            // Ensure the update runs on the UI thread
            Dispatcher.Invoke((Action)(() =>
            {
                ProgressBar.Value = value;
            }));
        }
    }
}