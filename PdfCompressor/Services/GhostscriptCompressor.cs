using System.Diagnostics;
using System.IO;

namespace PdfCompressor.Services
{
    /// <summary>
    /// Compresses PDF files using Ghostscript executable and quality settings.
    /// </summary>
    public class GhostscriptCompressor
    {
        private readonly string ghostscriptPath;

        /// <summary>
        /// Initializes a new instance of GhostscriptCompressor.
        /// </summary>
        /// <param name="ghostscriptExecutablePath">Path to Ghostscript executable (gswin64c.exe)</param>
        public GhostscriptCompressor(string ghostscriptExecutablePath)
        {
            ghostscriptPath = ghostscriptExecutablePath;
        }

        /// <summary>
        /// Compresses a PDF file using Ghostscript with a given quality level.
        /// </summary>
        /// <param name="inputPath">Path to the original PDF file</param>
        /// <param name="outputPath">Path to the compressed output PDF</param>
        /// <param name="quality">Quality level (0–100)</param>
        /// <returns>True if compression succeeded, false otherwise</returns>
        public bool Compress(string inputPath, string outputPath, int quality)
        {
            if (!File.Exists(ghostscriptPath)) return false;

            // Choose Ghostscript preset based on quality
            string qualityParam = quality <= 40 ? "/screen" :
                                  quality <= 70 ? "/ebook" :
                                  quality <= 90 ? "/printer" : "/prepress";

            string args = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS={qualityParam} " +
                          "-dNOPAUSE -dQUIET -dBATCH " +
                          $"-sOutputFile=\"{outputPath}\" \"{inputPath}\"";

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ghostscriptPath,
                        Arguments = args,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };

                process.Start();
                process.WaitForExit();

                return File.Exists(outputPath);
            }
            catch
            {
                return false;
            }
        }
    }
}
