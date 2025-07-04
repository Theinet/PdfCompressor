using System.IO;

namespace PdfCompressor.Models
{
    /// <summary>
    /// Represents a single PDF file with its full path and filename.
    /// Used for displaying and processing selected PDF files.
    /// </summary>
    public class PdfFileItem
    {
        /// <summary>
        /// Full path to the PDF file.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets the filename from the file path (e.g., "document.pdf").
        /// </summary>
        public string FileName => Path.GetFileName(FilePath);


    }
}
