using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace PdfCompressor
{
    /// <summary>
    /// A simple dialog window for entering page numbers (e.g. "1,3-5") to remove.
    /// </summary>
    public partial class PageInputDialog : Window
    {
        /// <summary>
        /// Gets the text input entered by the user or the action result ("even"/"odd").
        /// </summary>
        public string ResultText { get; private set; }

        public PageInputDialog()
        {
            InitializeComponent();

            // Set localized UI text for all visible controls
            this.Title = LocalizationManager.GetString("RemovePagesTitle");
            PromptText.Text = LocalizationManager.GetString("EnterPagesToRemove");
            OkButton.Content = LocalizationManager.GetString("OK");
            CancelButton.Content = LocalizationManager.GetString("Cancel");
            EvenOddLabel.Text = LocalizationManager.GetString("EvenOddPrompt");
            DeleteEvenOddButton.Content = LocalizationManager.GetString("Delete");

            InputBox.Focus();
        }

        /// <summary>
        /// Handles OK button click: stores input and closes the dialog.
        /// </summary>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            ResultText = InputBox.Text;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handles Cancel button click: closes the dialog without saving.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles Enter and Escape key input.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Ok_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Cancel_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the click on the "Delete even/odd pages" button.
        /// </summary>
        private void DeleteEvenOdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EvenOddChoiceDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                // Set result as "even" or "odd" and close dialog
                ResultText = dialog.Result;
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Adds fade-in animation when the dialog loads.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            this.BeginAnimation(Window.OpacityProperty, fadeIn);
        }
    }
}
