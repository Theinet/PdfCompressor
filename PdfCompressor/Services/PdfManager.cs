using PdfCompressor.Models;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;

namespace PdfCompressor.Services
{
    /// <summary>
    /// Provides high-level PDF manipulation operations such as splitting, merging, extracting and modifying pages.
    /// </summary>
    public class PdfManager
    {
        /// <summary>
        /// Extracts all text from the given PDF file and saves it to a .txt file.
        /// </summary>
        public void ExtractText(PdfFileItem selected, string outputPath, Action<string> log)
        {
            using var stream = new FileStream(selected.FilePath, FileMode.Open, FileAccess.Read);
            using var doc = new PdfLoadedDocument(stream);

            string result = "";
            for (int i = 0; i < doc.Pages.Count; i++)
            {
                result += $"--- Page {i + 1} ---\n{doc.Pages[i].ExtractText()}\n";
            }

            File.WriteAllText(outputPath, result);
            log($"{LocalizationManager.GetString("TextExtracted")}: {outputPath}");
            CustomMessageBox.Show(LocalizationManager.GetString("TextExtracted"));
        }
        /// <summary>
        /// Extracts all images from the given PDF file and saves them as PNG files.
        /// </summary>
        public int ExtractImages(PdfFileItem selected, string outputFolder, Action<string> log)
        {
            Directory.CreateDirectory(outputFolder);
            int count = 0;

            try
            {
                using var stream = new FileStream(selected.FilePath, FileMode.Open, FileAccess.Read);
                using var doc = new PdfLoadedDocument(stream);

                for (int i = 0; i < doc.Pages.Count; i++)
                {
                    var images = doc.Pages[i].ExtractImages();
                    if (images == null) continue;

                    foreach (System.Drawing.Image img in images)
                    {
                        try
                        {
                            string outputFile = Path.Combine(outputFolder, $"page_{i + 1}_img_{count + 1}.png");
                            img.Save(outputFile, ImageFormat.Png);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            CrashLogger.Log(ex);
                            log($"{LocalizationManager.GetString("Error")}: {ex.Message}");
                            MessageBox.Show($"Image save error:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            img.Dispose();
                        }
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                string successMessage = $"{LocalizationManager.GetString("ImagesExtracted")}: {count}";
                CustomMessageBox.Show(successMessage, LocalizationManager.GetString("Ready"));
                log(successMessage);
                return count;
            }
            catch (Exception ex)
            {
                CrashLogger.Log(ex);
                string errorMessage = $"{LocalizationManager.GetString("Error")}: {ex.Message}";
                CustomMessageBox.Show(errorMessage, LocalizationManager.GetString("Error"));
                log(errorMessage);
                return count;
            }
        }
        /// <summary>
        /// Splits a multi-page PDF into separate single-page PDF files.
        /// </summary>
        public void SplitPdf(PdfFileItem selected, string outputFolder, Action<string> log)
        {
            Directory.CreateDirectory(outputFolder);

            var inputDoc = PdfSharp.Pdf.IO.PdfReader.Open(selected.FilePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);

            for (int i = 0; i < inputDoc.PageCount; i++)
            {
                var newDoc = new PdfSharp.Pdf.PdfDocument();
                newDoc.Version = inputDoc.Version;
                newDoc.Info.Title = $"Page {i + 1}";
                newDoc.AddPage(inputDoc.Pages[i]);

                string file = Path.Combine(outputFolder, $"Page_{i + 1}.pdf");
                newDoc.Save(file);
                log($"Saved: {file}");
            }

            log($"{LocalizationManager.GetString("SplitDone")}: {outputFolder}");
            CustomMessageBox.Show(LocalizationManager.GetString("SplitDone"));
        }
        /// <summary>
        /// Removes specific pages from a PDF and saves the cleaned version.
        /// </summary>
        public void RemovePages(PdfFileItem selected, HashSet<int> pagesToRemove, string outputPath, Action<string> log)
        {
            var inputDoc = PdfSharp.Pdf.IO.PdfReader.Open(selected.FilePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
            var newDoc = new PdfSharp.Pdf.PdfDocument();

            for (int i = 0; i < inputDoc.PageCount; i++)
            {
                if (!pagesToRemove.Contains(i))
                {
                    newDoc.AddPage(inputDoc.Pages[i]);
                }
            }

            if (newDoc.PageCount == 0)
            {
                string msg = LocalizationManager.GetString("PdfEmptyError") ??
                             $"PDF \"{Path.GetFileName(selected.FilePath)}\" has no pages after removal. Skipped.";
                log(msg);
                CustomMessageBox.Show(msg, LocalizationManager.GetString("Info") ?? "Info");
                return;
            }

            newDoc.Save(outputPath);
            newDoc.Close();

            log($"{LocalizationManager.GetString("PagesDeleted")}: {outputPath}");
            CustomMessageBox.Show(LocalizationManager.GetString("PagesDeleted"));
        }
        /// <summary>
        /// Merges multiple PDF files into a single PDF and saves to output path.
        /// </summary>
        public void MergePdfs(List<PdfFileItem> files, string outputPath, Action<string> log)
        {
            try
            {
                var mergedDoc = new PdfSharp.Pdf.PdfDocument();

                foreach (var file in files)
                {
                    var inputDoc = PdfSharp.Pdf.IO.PdfReader.Open(file.FilePath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);

                    for (int i = 0; i < inputDoc.PageCount; i++)
                    {
                        var sourcePage = inputDoc.Pages[i];
                        var newPage = mergedDoc.AddPage();

                        // Set A4 page size
                        newPage.Width = PdfSharp.Drawing.XUnit.FromMillimeter(210);
                        newPage.Height = PdfSharp.Drawing.XUnit.FromMillimeter(297);

                        using (var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(newPage))
                        using (var form = PdfSharp.Drawing.XPdfForm.FromFile(file.FilePath))
                        {
                            form.PageNumber = i + 1;
                            var rect = new PdfSharp.Drawing.XRect(0, 0, newPage.Width.Point, newPage.Height.Point);
                            gfx.DrawImage(form, rect);
                        }
                    }
                }

                mergedDoc.Save(outputPath);
                mergedDoc.Close();

                log($"{LocalizationManager.GetString("Ready")}: {outputPath}");
                CustomMessageBox.Show(LocalizationManager.GetString("MergedCreated"), LocalizationManager.GetString("Ready"));
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"{LocalizationManager.GetString("Error")}: {ex.Message}", LocalizationManager.GetString("Error"));
                log($"{LocalizationManager.GetString("Error")}: {ex.Message}");
            }
        }
        /// <summary>
        /// Returns the number of pages in the PDF file.
        /// </summary>
        public int GetPageCount(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var doc = new PdfLoadedDocument(stream);
            return doc.Pages.Count;
        }
        /// <summary>
        /// Compresses multiple PDF files using Ghostscript and saves them to a specified output folder.
        /// </summary>
        public void CompressPdf(
            List<PdfFileItem> files,
            int quality,
            Action<string> log,
            IProgress<int> progress,
            string outputFolder)
        {
            string gsExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ghostscript", "gswin64c.exe");

            if (!File.Exists(gsExe))
            {
                log(LocalizationManager.GetString("GhostscriptNotFound") ?? "❌ Ghostscript executable not found.");
                return;
            }

            int compressedCount = 0;

            for (int i = 0; i < files.Count; i++)
            {
                var inputPath = files[i].FilePath;
                var fileName = Path.GetFileNameWithoutExtension(inputPath);
                var compressedPath = Path.Combine(outputFolder, fileName + "_compressed.pdf");

                try
                {
                    long originalSize = new FileInfo(inputPath).Length;

                    if (originalSize < 100 * 1024)
                    {
                        log($"[{i + 1}/{files.Count}] {string.Format(LocalizationManager.GetString("CompressionSkippedSmall") ?? "Skipped \"{0}\": file too small", fileName)}");
                        File.Copy(inputPath, compressedPath, true);
                        progress.Report(i + 1);
                        continue;
                    }

                    int resolution = quality >= 90 ? 200 : quality >= 75 ? 150 : 100;
                    int jpegQ = Math.Max(30, Math.Min(quality, 95));

                    string args = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 " +
                                  "-dPDFSETTINGS=/default " +
                                  "-dAutoFilterColorImages=false -dAutoFilterGrayImages=false " +
                                  "-dColorImageFilter=/DCTEncode -dGrayImageFilter=/DCTEncode " +
                                  "-dDownsampleColorImages=true -dDownsampleGrayImages=true " +
                                  $"-dColorImageResolution={resolution} -dGrayImageResolution={resolution} " +
                                  $"-dJPEGQ={jpegQ} " +
                                  "-dMonoImageDownsampleType=/Subsample -dDownsampleMonoImages=true -dMonoImageResolution=300 " +
                                  "-dNOPAUSE -dQUIET -dBATCH " +
                                  $"-sOutputFile=\"{compressedPath}\" \"{inputPath}\"";

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = gsExe,
                            Arguments = args,
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };

                    process.Start();
                    process.WaitForExit();

                    if (File.Exists(compressedPath))
                    {
                        long compressedSize = new FileInfo(compressedPath).Length;

                        if (compressedSize >= originalSize * 0.95)
                        {
                            File.Delete(compressedPath);
                            File.Copy(inputPath, compressedPath, true);

                            log($"[{i + 1}/{files.Count}] " +
                                string.Format(LocalizationManager.GetString("CompressionSkipped") ?? "Skipped \"{0}\": already compressed or not compressible.", fileName));
                        }
                        else
                        {
                            log($"[{i + 1}/{files.Count}] " +
                                string.Format(LocalizationManager.GetString("Compressed") ?? "Compressed \"{0}\" from {1} to {2}",
                                              fileName,
                                              FormatSize(originalSize),
                                              FormatSize(compressedSize)));
                            compressedCount++;
                        }
                    }
                    else
                    {
                        log($"[{i + 1}/{files.Count}] ❌ {LocalizationManager.GetString("CompressionFailed") ?? "Compression failed"} \"{fileName}\"");
                    }
                }
                catch (Exception ex)
                {
                    log($"[{i + 1}/{files.Count}] ❌ {LocalizationManager.GetString("Error") ?? "Error"}: {ex.Message}");
                }

                progress.Report(i + 1);
            }

            log($"{LocalizationManager.GetString("CompressionCompleted") ?? "Compression completed"} ({compressedCount}/{files.Count})");
        }

        private string FormatSize(long bytes)
        {
            if (bytes >= 1_000_000_000)
                return $"{bytes / 1_000_000_000.0:F2} GB";
            if (bytes >= 1_000_000)
                return $"{bytes / 1_000_000.0:F2} MB";
            if (bytes >= 1_000)
                return $"{bytes / 1_000.0:F2} KB";
            return $"{bytes} B";
        }
        /// <summary>
        /// Converts PDF pages to images in the specified format and saves them to output folder.
        /// </summary>
        public void ConvertPdfToImages(string pdfPath, string outputFolder, string format, int dpi, Action<string> log)
        {
            Directory.CreateDirectory(outputFolder);

            if (dpi < 30 || dpi > 1200)
            {
                throw new ArgumentException($"{LocalizationManager.GetString("Error")}: {string.Format(LocalizationManager.GetString("InvalidDpiRange"), dpi)}");
            }

            using var doc = PdfiumViewer.PdfDocument.Load(pdfPath);

            ImageFormat imageFormat = format.ToLower() switch
            {
                "png" => ImageFormat.Png,
                "jpg" => ImageFormat.Jpeg,
                "jpeg" => ImageFormat.Jpeg,
                "bmp" => ImageFormat.Bmp,
                "tiff" => ImageFormat.Tiff,
                _ => throw new ArgumentException(LocalizationManager.GetString("UnsupportedFormat") + ": " + format)
            };

            for (int page = 0; page < doc.PageCount; page++)
            {
                var pageSize = doc.PageSizes[page];
                int widthPx = (int)(pageSize.Width * dpi / 72f);
                int heightPx = (int)(pageSize.Height * dpi / 72f);

                if (widthPx <= 0 || heightPx <= 0)
                {
                    log($"{LocalizationManager.GetString("Error")}: Invalid image size {widthPx}x{heightPx} on page {page + 1}");
                    continue;
                }

                log($"{LocalizationManager.GetString("RenderingPage")}: {page + 1}, {widthPx}x{heightPx} @ {dpi} DPI");

                using var image = doc.Render(page, widthPx, heightPx, dpi, dpi, true);
                string fileName = $"page_{page + 1}.{format}";
                string outputPath = Path.Combine(outputFolder, fileName);

                if (imageFormat == ImageFormat.Jpeg)
                {
                    var encoders = ImageCodecInfo.GetImageEncoders();
                    var encoder = encoders.FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    if (encoder != null)
                    {
                        var encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                        image.Save(outputPath, encoder, encoderParams);
                    }
                    else
                    {
                        log($"{LocalizationManager.GetString("Error")}: JPEG encoder not found.");
                    }
                }
                else
                {
                    image.Save(outputPath, imageFormat);
                }
            }

            log(string.Format(LocalizationManager.GetString("PdfConvertedSuccess"), format.ToUpper()));
        }
    }
}