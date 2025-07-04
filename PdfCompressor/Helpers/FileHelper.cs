using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfCompressor.Helpers
{
    /// <summary>
    /// Provides helper methods for working with files and paths.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Sorts a list of file paths numerically based on digits in the filenames.
        /// Files without numbers are sorted to the end.
        /// </summary>
        /// <param name="files">A collection of file paths</param>
        /// <returns>A numerically sorted list of file paths</returns>
        public static List<string> SortFilesNumerically(IEnumerable<string> files)
        {
            return files.OrderBy(f =>
            {
                var name = Path.GetFileNameWithoutExtension(f);
                var digits = new string(name.Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out int n) ? n : int.MaxValue;
            }).ToList();
        }

        /// <summary>
        /// Returns an absolute path to a file placed on the user's desktop.
        /// </summary>
        /// <param name="filename">The name of the file (with extension)</param>
        /// <returns>Full file path on the desktop</returns>
        public static string GetDesktopPath(string filename)
        {
            return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                filename
            );
        }
    }
}
