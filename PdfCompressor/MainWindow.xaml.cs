using PdfCompressor.Services;
using PdfCompressor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PdfCompressor.Helpers;
using System.Windows.Data;


namespace PdfCompressor
{
     public class EmptyListToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is int count && count == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        // List of PDF files currently loaded into the app
        private List<PdfFileItem> pdfFiles = new List<PdfFileItem>();

        // PDF processing service instance
        private readonly PdfManager pdfManager = new PdfManager();

        public MainWindow()
        {
            InitializeComponent();
            ApplyLocalization(); // Load localized UI strings
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (SecurityHelper.IsRunAsAdmin())
            {
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                timer.Tick += (s, args) =>
                {
                    timer.Stop();

                    string msg = LocalizationManager.GetString("RunAsAdminWarningMessage") ??
                                 "The app is running with administrator privileges.";
                    string caption = LocalizationManager.GetString("Info") ?? "Info";

                    Log(LocalizationManager.GetString("RunAsAdminWarningLog") ?? "[INFO] App started as admin.");
                    CustomMessageBox.Show(msg, caption);

                    var dlg = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = "PDF files (*.pdf)|*.pdf",
                        Multiselect = true
                    };

                    if (dlg.ShowDialog() == true)
                        AddFiles(dlg.FileNames);
                };

                timer.Start();
            }
        }
        /// <summary>
        /// Handles the "Merge" button click: merges multiple selected PDFs into one file.
        /// </summary>
        private async void Merge_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count < 2)
            {
                CustomMessageBox.Show(
                    LocalizationManager.GetString("SelectAtLeastTwo"),
                    LocalizationManager.GetString("Error")
                );
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = "Merged.pdf"
            };

            if (dialog.ShowDialog() != true)
                return;

            string outputPath = dialog.FileName;

            await RunWithAutoLoading(async () =>
            {
                await Task.Run(() =>
                {
                    pdfManager.MergePdfs(pdfFiles, outputPath, Log);
                    Log($"{LocalizationManager.GetString("MergedCreated")}: {outputPath}");
                });
            });
        }
        // Apply UI text from localization resource
        private void ApplyLocalization()
        {
            // Update all UI elements with localized strings
            MenuFile.Header = LocalizationManager.GetString("MenuFile");
            MenuAddPdf.Header = LocalizationManager.GetString("MenuAddPdf");
            MenuExit.Header = LocalizationManager.GetString("MenuExit");
            MenuPrint.Header = LocalizationManager.GetString("Print");
            MenuActions.Header = LocalizationManager.GetString("MenuActions");
            MenuCompressPdf.Header = LocalizationManager.GetString("MenuCompressPdf");
            MenuSafeMerge.Header = LocalizationManager.GetString("MenuSafeMerge");
            MenuSplitPdf.Header = LocalizationManager.GetString("MenuSplitPdf");
            MenuRemovePages.Header = LocalizationManager.GetString("MenuRemovePages");
            MenuExtractText.Header = LocalizationManager.GetString("MenuExtractText");
            MenuExtractImages.Header = LocalizationManager.GetString("MenuExtractImages");
            MenuRemoveAll.Header = LocalizationManager.GetString("MenuRemoveAll");
            MenuClearLog.Header = LocalizationManager.GetString("MenuClearLog");
            MenuLanguage.Header = LocalizationManager.GetString("MenuLanguage");
            MenuHelp.Header = LocalizationManager.GetString("MenuHelp");
            MenuAbout.Header = LocalizationManager.GetString("MenuAbout");
            MenuConvertToImages.Header = LocalizationManager.GetString("MenuConvertToImages");
            TextCompressionQuality.Text = LocalizationManager.GetString("CompressionQuality");
            ButtonCompress.Content = LocalizationManager.GetString("ButtonCompress");
            bool isAdmin = SecurityHelper.IsRunAsAdmin();
            string hintKey = isAdmin ? "DragHintAdmin" : "DropHere";
            PdfListHint.Text = LocalizationManager.GetString(hintKey) ??
                               (isAdmin ? "Drag and drop doesn't work in admin mode"
                                        : "Drag and drop PDF files here");
        }
        // Set UI language and reload window
        private void SetCulture(string lang)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);

            var newWindow = new MainWindow { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            newWindow.Show();
            Close();
        }
        // Language switch buttons
        private void SetEnglish_Click(object sender, RoutedEventArgs e) => SetCulture("en");
        private void SetUkrainian_Click(object sender, RoutedEventArgs e) => SetCulture("uk");
        // Add selected files to the list
        private void AddFiles(IEnumerable<string> files)
        {
            var sortedFiles = files.OrderBy(f =>
            {
                var name = Path.GetFileNameWithoutExtension(f);
                var digits = new string(name.Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out int n) ? n : int.MaxValue;
            }).ToList();

            foreach (var file in sortedFiles)
            {
                if (Path.GetExtension(file).ToLower() != ".pdf")
                {
                    Log($"{LocalizationManager.GetString("Error")}: {file}");
                    continue;
                }

                if (!pdfFiles.Any(f => f.FilePath == file))
                {
                    var item = new PdfFileItem { FilePath = file };
                    pdfFiles.Add(item);
                    PdfListBox.Items.Add(item);
                    Log($"{LocalizationManager.GetString("Added")}: {file}");
                }
            }
        }

    // Remove selected file from list
    private void RemoveFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is PdfFileItem item)
            {
                pdfFiles.Remove(item);
                PdfListBox.Items.Remove(item);
                Log($"{LocalizationManager.GetString("Removed")}: {item.FileName}");
            }
        }
        // Remove all files from list
        private void RemoveAllFiles_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("AlreadyEmpty"), LocalizationManager.GetString("Info"));
                return;
            }

            pdfFiles.Clear();
            PdfListBox.Items.Clear();
            Log(LocalizationManager.GetString("AllFilesRemoved"));
        }
        // Clear log textbox
        private void ClearLog_Click(object sender, RoutedEventArgs e) => LogTextBox.Clear();
        // Exit application
        private void Exit_Click(object sender, RoutedEventArgs e) => Close();
        // Keyboard shortcuts for common actions
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.O: AddPdf_Click(null, null); break;
                    case System.Windows.Input.Key.L: ClearLog_Click(null, null); break;
                    case System.Windows.Input.Key.X: RemoveAllFiles_Click(null, null); break;
                    case System.Windows.Input.Key.S: Compress_Click(null, null); break;
                    case System.Windows.Input.Key.P: Print_Click(null, null); break;
                }
            }
        }
        // Start PDF compression
        private async void Compress_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("AddPdfFirst"), LocalizationManager.GetString("Error"));
                return;
            }

            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = LocalizationManager.GetString("SelectOutputFolder"),
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string basePath = dlg.SelectedPath;
            string outputFolder = Path.Combine(basePath, "Compressed");
            Directory.CreateDirectory(outputFolder);

            ProgressBar.Value = 0;
            ProgressBar.Maximum = pdfFiles.Count;
            Log(LocalizationManager.GetString("CompressionStarted"));

            int quality = 60;
            var progress = new Progress<int>(value => ProgressBar.Value = value);

            LoadingOverlay.Visibility = Visibility.Visible;
            LoadingText.Text = LocalizationManager.GetString("Loading");

            await Task.Run(() =>
            {
                pdfManager.CompressPdf(pdfFiles, quality, Log, progress, outputFolder);
            });

            LoadingText.Text = LocalizationManager.GetString("Ready");
            await Task.Delay(1000);
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
        private async void ConvertToImages_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("AddPdfFirst"), LocalizationManager.GetString("Error"));
                return;
            }

            PdfFileItem selected = pdfFiles.Count == 1 ? pdfFiles[0] : PdfListBox.SelectedItem as PdfFileItem;

            if (selected == null)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("SelectOneFile"), LocalizationManager.GetString("Error"));
                return;
            }

            var folderDlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = LocalizationManager.GetString("SelectOutputFolder"),
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            var dialog = new ImageExportDialog { Owner = this };
            if (dialog.ShowDialog() != true)
                return;

            string format = ((ComboBoxItem)dialog.FormatComboBox.SelectedItem).Content.ToString().ToLower();
            if (!int.TryParse(dialog.DpiBox.Text, out int dpi))
                dpi = 350;

            string outputFolder = Path.Combine(folderDlg.SelectedPath, "ImagesExported");
            Directory.CreateDirectory(outputFolder);

            await RunWithAutoLoading(async () =>
            {
                await Task.Run(() =>
                {
                    try
                    {
                        pdfManager.ConvertPdfToImages(selected.FilePath, outputFolder, format, dpi, Log);
                        Log($"{LocalizationManager.GetString("ImagesExportedTo") ?? "Images exported to"}: {outputFolder}");
                    }
                    catch (Exception ex)
                    {
                        Log($"{LocalizationManager.GetString("Error")}: {ex.Message}");
                        Dispatcher.Invoke(() =>
                        {
                            CustomMessageBox.Show($"{LocalizationManager.GetString("Error")}: {ex.Message}", LocalizationManager.GetString("Error"));
                        });
                    }
                });
            });
        }
        // Print selected PDF file
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("AddPdfFirst"), LocalizationManager.GetString("Error"));
                return;
            }

            PdfFileItem selected;

            if (pdfFiles.Count == 1)
            {
                selected = pdfFiles[0];
            }
            else
            {
                selected = PdfListBox.SelectedItem as PdfFileItem;
                if (selected == null)
                {
                    CustomMessageBox.Show(LocalizationManager.GetString("SelectOneFile"), LocalizationManager.GetString("Error"));
                    return;
                }
            }

            try
            {
                var printProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = selected.FilePath,
                        Verb = "print",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true
                    }
                };
                printProcess.Start();
                Log($"{LocalizationManager.GetString("PrintSent")}: {selected.FileName}");
            }
            catch (System.ComponentModel.Win32Exception)
            {
                string msg = LocalizationManager.GetString("NoAppAssociated");
                CustomMessageBox.Show(msg, LocalizationManager.GetString("Error"));
                Log($"[Print Error] {msg}");
            }
            catch (Exception ex)
            {
                string msg = $"{LocalizationManager.GetString("PrintError")}: {ex.Message}";
                CustomMessageBox.Show(msg, LocalizationManager.GetString("Error"));
                Log(msg);
            }
        }
        // Extract text from selected PDF
        private async void ExtractText_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("AddPdfFirst"), LocalizationManager.GetString("Error"));
                return;
            }

            PdfFileItem selected = pdfFiles.Count == 1 ? pdfFiles[0] : PdfListBox.SelectedItem as PdfFileItem;
            if (selected == null)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("SelectOneFile"), LocalizationManager.GetString("Error"));
                return;
            }

            var folderDlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = LocalizationManager.GetString("SelectOutputFolder"),
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string basePath = folderDlg.SelectedPath;
            string outputFolder = Path.Combine(basePath, "ExtractedText");
            Directory.CreateDirectory(outputFolder);

            string outputPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(selected.FileName) + "_ExtractedText.txt");

            await RunWithAutoLoading(async () =>
            {
                await Task.Run(() =>
                {
                    pdfManager.ExtractText(selected, outputPath, Log);
                });
            });
        }
        // Extract images from selected PDF
        private async void ExtractImages_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("AddPdfFirst"), LocalizationManager.GetString("Error"));
                return;
            }

            PdfFileItem selected = pdfFiles.Count == 1 ? pdfFiles[0] : PdfListBox.SelectedItem as PdfFileItem;
            if (selected == null)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("SelectOneFile"), LocalizationManager.GetString("Error"));
                return;
            }

            var folderDlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = LocalizationManager.GetString("SelectOutputFolder"),
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string basePath = folderDlg.SelectedPath;
            string outputFolder = Path.Combine(basePath, "ExtractedImages");
            Directory.CreateDirectory(outputFolder);

            int extractedCount = 0;

            await RunWithAutoLoading(async () =>
            {
                try
                {
                    await Task.Run(() =>
                    {
                        extractedCount = pdfManager.ExtractImages(selected, outputFolder, Log);
                        Log($"{LocalizationManager.GetString("ImagesExtracted")}: {extractedCount} {outputFolder}");
                    });
                }
                catch (Exception ex)
                {
                    Log($"[ExtractImages Error] {ex.Message}");
                    Dispatcher.Invoke(() =>
                    {
                        CustomMessageBox.Show($"{LocalizationManager.GetString("Error")}: {ex.Message}", LocalizationManager.GetString("Error"));
                    });
                }
            });

            Dispatcher.Invoke(() =>
            {
                LoadingText.Text = LocalizationManager.GetString("Ready");
            }
            );
        }
        // Split PDF into separate pages
        private async void Split_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("AddPdfFirst"), LocalizationManager.GetString("Error"));
                return;
            }

            PdfFileItem selected = pdfFiles.Count == 1 ? pdfFiles[0] : PdfListBox.SelectedItem as PdfFileItem;
            if (selected == null)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("SelectOneFile"), LocalizationManager.GetString("Error"));
                return;
            }

            var folderDlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = LocalizationManager.GetString("SelectOutputFolder"),
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string basePath = folderDlg.SelectedPath;
            string outputPath = Path.Combine(basePath, "SplitPages");
            Directory.CreateDirectory(outputPath);

            await RunWithAutoLoading(async () =>
            {
                await Task.Run(() =>
                {
                    pdfManager.SplitPdf(selected, outputPath, fileName =>
                    {
                        Log(fileName);
                    });
                });
            });
        }
        // Remove selected pages from PDF
        private void RemovePages_Click(object sender, RoutedEventArgs e)
        {
            if (pdfFiles.Count == 0)
            {
                CustomMessageBox.Show(LocalizationManager.GetString("AddPdfFirst"), LocalizationManager.GetString("Error"));
                return;
            }

            PdfFileItem selected;
            if (pdfFiles.Count == 1)
            {
                selected = pdfFiles[0];
            }
            else
            {
                selected = PdfListBox.SelectedItem as PdfFileItem;
                if (selected == null)
                {
                    CustomMessageBox.Show(LocalizationManager.GetString("SelectOneFile"), LocalizationManager.GetString("Error"));
                    return;
                }
            }

            var dialog = new PageInputDialog { Owner = this };
            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.ResultText))
                return;

            HashSet<int> pagesToRemove;

            if (dialog.ResultText == "even" || dialog.ResultText == "odd")
            {
                int totalPages = pdfManager.GetPageCount(selected.FilePath);
                pagesToRemove = new HashSet<int>();

                for (int i = 0; i < totalPages; i++)
                {
                    bool isEven = (i + 1) % 2 == 0;
                    if ((dialog.ResultText == "even" && isEven) || (dialog.ResultText == "odd" && !isEven))
                        pagesToRemove.Add(i);
                }
            }
            else
            {
                pagesToRemove = ParsePageRanges(dialog.ResultText);
            }

            var folderDlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = LocalizationManager.GetString("SelectOutputFolder"),
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string basePath = folderDlg.SelectedPath;
            string outputFolder = Path.Combine(basePath, "Cleaned");
            Directory.CreateDirectory(outputFolder);

            string output = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(selected.FileName) + "_cleaned.pdf");

            pdfManager.RemovePages(selected, pagesToRemove, output, Log);
        }
        private void ShowLoadingOverlay()
        {
            LoadingText.Text = LocalizationManager.GetString("Loading") ?? "Loading, please wait...";
            LoadingOverlay.Visibility = Visibility.Visible;
            IsEnabled = false;
        }
        private void HideLoadingOverlay()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            IsEnabled = true;
        }
        /// <summary>
        /// Handles the Add PDF button click.
        /// Opens a file dialog for the user to select PDF files,
        /// shows a loading overlay while processing,
        /// and adds the selected files to the list after a simulated delay.
        /// </summary>
        private async void AddPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Multiselect = true
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                string[] files = dlg.FileNames;

                await Task.Run(() =>
                {
                    var sortedFiles = files.OrderBy(f =>
                    {
                        var name = Path.GetFileNameWithoutExtension(f);
                        var digits = new string(name.Where(char.IsDigit).ToArray());
                        return int.TryParse(digits, out int n) ? n : int.MaxValue;
                    });

                    foreach (var file in sortedFiles)
                    {
                        if (Path.GetExtension(file).ToLower() != ".pdf")
                        {
                            Dispatcher.Invoke(() =>
                                Log($"{LocalizationManager.GetString("Error")}: {file}"));
                            continue;
                        }

                        if (pdfFiles.Any(f => f.FilePath == file))
                            continue;

                        var item = new PdfFileItem { FilePath = file };

                        Dispatcher.Invoke(() =>
                        {
                            pdfFiles.Add(item);
                            PdfListBox.Items.Add(item);
                            Log($"{LocalizationManager.GetString("Added")}: {file}");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{LocalizationManager.GetString("Error")}: {ex.Message}", LocalizationManager.GetString("Error"));
                Log($"[AddPdf Error] {ex.Message}");
            }
        }
        // Parse user input like "1,3-5" into page indexes
        private HashSet<int> ParsePageRanges(string input)
        {
            var result = new HashSet<int>();
            var parts = input.Split(',');

            foreach (var part in parts)
            {
                if (part.Contains("-"))
                {
                    var bounds = part.Split('-');
                    if (bounds.Length == 2 &&
                        int.TryParse(bounds[0], out int start) &&
                        int.TryParse(bounds[1], out int end))
                    {
                        for (int i = start; i <= end; i++)
                            result.Add(i - 1); // Pages are zero-indexed
                    }
                }
                else if (int.TryParse(part.Trim(), out int single))
                    result.Add(single - 1);
            }

            return result;
        }
        // Open app homepage
        private void About_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://theinet.vercel.app/pdfcompressor",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{LocalizationManager.GetString("Error")}: {ex.Message}", LocalizationManager.GetString("Error"));
                Log($"{LocalizationManager.GetString("Error")}: {ex.Message}");
            }
        }
        // Write a message to the log textbox
        private void Log(string msg)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Log(msg));
                return;
            }

            LogTextBox.AppendText($"{DateTime.Now:T} — {msg}\n");
            LogTextBox.ScrollToEnd();
        }
        // Handle drag-and-drop of files into the window
        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                AddFiles((string[])e.Data.GetData(System.Windows.DataFormats.FileDrop));
        }

        private void Window_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop) ? System.Windows.DragDropEffects.Copy : System.Windows.DragDropEffects.None;
            e.Handled = true;
        }
        // Handle delete/move up/down in the list using keyboard
        private void PdfListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int index = PdfListBox.SelectedIndex;
            if (index == -1) return;

            if (e.Key == System.Windows.Input.Key.Delete && PdfListBox.SelectedItem is PdfFileItem item)
            {
                pdfFiles.Remove(item);
                PdfListBox.Items.Remove(item);
                Log($"{LocalizationManager.GetString("Removed")}: {item.FileName}");
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up && index > 0)
            {
                SwapItems(index, index - 1);
                PdfListBox.SelectedIndex = index - 1;
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Down && index < PdfListBox.Items.Count - 1)
            {
                SwapItems(index, index + 1);
                PdfListBox.SelectedIndex = index + 1;
                e.Handled = true;
            }
        }
        // Swap two items in the list
        private void SwapItems(int i1, int i2)
        {
            (pdfFiles[i1], pdfFiles[i2]) = (pdfFiles[i2], pdfFiles[i1]);
            (PdfListBox.Items[i1], PdfListBox.Items[i2]) = (PdfListBox.Items[i2], PdfListBox.Items[i1]);
            Log(LocalizationManager.GetString("ItemsSwapped") ?? "Items swapped.");
        }
        private async Task RunWithAutoLoading(Func<Task> operation, int delayBeforeShowing = 300)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            bool loadingShown = false;

            var delayTask = Task.Delay(delayBeforeShowing, cts.Token).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingText.Text = LocalizationManager.GetString("Loading") ?? "Loading...";
                    ShowLoadingOverlay();
                });
                loadingShown = true;
            }, TaskScheduler.Default);

            try
            {
                await operation();
            }
            finally
            {
                cts.Cancel();

                if (loadingShown)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadingText.Text = LocalizationManager.GetString("Ready") ?? "Ready!";
                    });

                    await Task.Delay(600);

                    Dispatcher.Invoke(HideLoadingOverlay);
                }
            }
        }
    }
}