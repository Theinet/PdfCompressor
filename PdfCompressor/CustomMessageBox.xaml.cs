using System.Windows;

namespace PdfCompressor
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string title, string message)
        {
            InitializeComponent();

            TitleText.Text = string.IsNullOrWhiteSpace(title)
                ? LocalizationManager.GetString("Info")
                : title;

            MessageText.Text = string.IsNullOrWhiteSpace(message)
                ? "..."
                : message;

            OKButton.Content = LocalizationManager.GetString("OK");
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded && IsModal())
                DialogResult = true;

            Close();
        }

        public static void Show(string message, string title = null)
        {
            void ShowBox()
            {
                string finalTitle = string.IsNullOrWhiteSpace(title)
                    ? LocalizationManager.GetString("Info")
                    : title;

                var box = new CustomMessageBox(finalTitle, message);

                var main = Application.Current?.MainWindow;

                if (main != null && main != box && main != box.Owner)
                {
                    box.Owner = main;
                    box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                box.ShowDialog();
            }

            if (Application.Current.Dispatcher.CheckAccess())
                ShowBox();
            else
                Application.Current.Dispatcher.Invoke(ShowBox);
        }

        public static void ShowNonBlocking(string message, string title = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string finalTitle = string.IsNullOrWhiteSpace(title)
                    ? LocalizationManager.GetString("Info")
                    : title;

                var box = new CustomMessageBox(finalTitle, message);

                var main = Application.Current?.MainWindow;

                if (main != null && main != box && main != box.Owner)
                {
                    box.Owner = main;
                    box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    box.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                box.Show();
            });
        }

        private bool IsModal()
        {
            try
            {
                var field = typeof(Window).GetField("_showingAsDialog", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                return field != null && (bool)(field.GetValue(this) ?? false);
            }
            catch
            {
                return false;
            }
        }
    }
}
