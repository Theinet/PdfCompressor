using System;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Forms;
using PdfiumViewer;

namespace PdfCompressor.Services
{
    public class FileService : IFileService
    {
        public void PrintFile(string filePath, Action<string> log = null)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    string msg = $"{Path.GetFileName(filePath)} - {LocalizationManager.GetString("Error")}";
                    ShowMessage(msg, LocalizationManager.GetString("Error"));
                    log?.Invoke($"[Print Error] {msg}");
                    return;
                }

                if (!HasPhysicalPrinters())
                {
                    string msg = LocalizationManager.GetString("NoRealPrinters") ?? "No physical printer found.";
                    ShowMessage(msg, LocalizationManager.GetString("Error"));
                    log?.Invoke($"[Print Error] {msg}");
                    return;
                }

                using var document = PdfDocument.Load(filePath);
                using var printDoc = document.CreatePrintDocument(PdfPrintMode.ShrinkToMargin);

                printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                var printDialog = new PrintDialog
                {
                    AllowSomePages = true,
                    AllowSelection = true,
                    UseEXDialog = true,
                    Document = printDoc
                };

                if (printDialog.ShowDialog() != DialogResult.OK)
                {
                    log?.Invoke("[Print] Cancelled by user.");
                    return;
                }

                printDoc.PrinterSettings = printDialog.PrinterSettings;
                printDoc.DefaultPageSettings = printDialog.PrinterSettings.DefaultPageSettings;
                printDoc.PrintController = new StandardPrintController();
                printDoc.Print();

                string sent = $"{LocalizationManager.GetString("PrintSent") ?? "Print sent"}: {Path.GetFileName(filePath)}";
                log?.Invoke(sent);
            }
            catch (Exception ex)
            {
                string msg = $"{LocalizationManager.GetString("PrintError") ?? "Print error"}: {ex.Message}";
                ShowMessage(msg, LocalizationManager.GetString("Error"));
                log?.Invoke($"[Print Error] {msg}");
            }
        }

        private bool HasPhysicalPrinters()
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                string lower = printer.ToLower();
                if (!lower.Contains("pdf") && !lower.Contains("onenote") && !lower.Contains("xps") && !lower.Contains("fax") && !lower.Contains("microsoft"))
                    return true;
            }
            return false;
        }

        public void ShowMessage(string message, string caption = null)
        {
            CustomMessageBox.Show(message, caption ?? LocalizationManager.GetString("Info"));
        }
    }
}
