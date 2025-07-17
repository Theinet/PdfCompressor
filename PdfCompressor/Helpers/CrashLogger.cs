using System;
using System.IO;

namespace PdfCompressor
{
    public static class CrashLogger
    {
        public static void Log(Exception ex)
        {
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "PdfCompressor");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string logFile = Path.Combine(dir, "log.txt");
                File.AppendAllText(logFile,
                    $"[{DateTime.Now}] {ex.Message}\n{ex.StackTrace}\n\n");
            }
            catch
            {
            }
        }
    }
}
