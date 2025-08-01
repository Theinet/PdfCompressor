using System.IO;

namespace PdfCompressor.Helpers
{
    public static class LanguageHelper
    {
        private static readonly string FilePath = Path.Combine(Path.GetTempPath(), "pdfcompressor.lang");

        public static void SaveLanguage(string lang)
        {
            try
            {
                File.WriteAllText(FilePath, lang);
            }
            catch { }
        }

        public static string LoadLanguage()
        {
            try
            {
                if (File.Exists(FilePath))
                    return File.ReadAllText(FilePath);
            }
            catch { }

            return "en-US";
        }
    }
}
