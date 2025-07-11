using System.Windows;
using System.Windows.Input;

namespace PdfCompressor
{
    /// <summary>
    /// A dialog for choosing between removing even or odd pages.
    /// </summary>
    public partial class EvenOddChoiceDialog : Window
    {
        /// <summary>
        /// The selected result: "even", "odd", or null if canceled.
        /// </summary>
        public string Result { get; private set; }

        public EvenOddChoiceDialog()
        {
            InitializeComponent();

            // Set localized UI text
            this.Title = LocalizationManager.GetString("RemovePagesTitle");
            PromptText.Text = LocalizationManager.GetString("PromptEvenOdd");
            EvenButton.Content = LocalizationManager.GetString("EvenPages");
            OddButton.Content = LocalizationManager.GetString("OddPages");
            CancelButton.Content = LocalizationManager.GetString("Cancel");

            this.Loaded += (s, e) => EvenButton.Focus();
        }

        /// <summary>
        /// Called when the "Even" button is clicked.
        /// Sets the result to "even" and closes the dialog.
        /// </summary>
        private void Even_Click(object sender, RoutedEventArgs e)
        {
            Result = "even";
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Called when the "Odd" button is clicked.
        /// Sets the result to "odd" and closes the dialog.
        /// </summary>
        private void Odd_Click(object sender, RoutedEventArgs e)
        {
            Result = "odd";
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Called when the "Cancel" button is clicked.
        /// Closes the dialog without saving the selection.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles Enter and Escape keys for accessibility.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                Cancel_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (EvenButton.IsFocused)
                    Even_Click(this, new RoutedEventArgs());
                else if (OddButton.IsFocused)
                    Odd_Click(this, new RoutedEventArgs());
                else
                    Cancel_Click(this, new RoutedEventArgs());

                e.Handled = true;
            }
        }
    }
}
