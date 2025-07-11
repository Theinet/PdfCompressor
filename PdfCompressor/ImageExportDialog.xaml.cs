using System.Windows;
using System.Windows.Controls;

namespace PdfCompressor
{
    public partial class ImageExportDialog : Window
    {
        public string SelectedFormat { get; private set; } = "png";
        public int Dpi { get; private set; } = 350;

        public ImageExportDialog()
        {
            InitializeComponent();

            DpiBox.Text = "350";
            Title = LocalizationManager.GetString("ImageExport_Title");
            FormatLabel.Text = LocalizationManager.GetString("ImageExport_FormatLabel");
            DpiLabel.Text = LocalizationManager.GetString("ImageExport_DpiLabel");
            OKButton.Content = LocalizationManager.GetString("OK");
            CancelButton.Content = LocalizationManager.GetString("Cancel");
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (FormatComboBox.SelectedItem is ComboBoxItem selectedItem)
                SelectedFormat = selectedItem.Content.ToString().ToLower();

            if (int.TryParse(DpiBox.Text, out int dpi))
                Dpi = dpi;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}