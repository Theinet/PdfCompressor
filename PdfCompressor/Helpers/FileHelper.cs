using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace PdfCompressor.Helpers
{
    public static class FileHelper
    {
        public static string DataDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PdfCompressor");

        public static void SetupAppDataDirectory()
        {
            try
            {
                if (!Directory.Exists(DataDir))
                    Directory.CreateDirectory(DataDir);

                CopyIcon();
            }
            catch (Exception ex)
            {
                CrashLogger.Log(ex);
                MessageBox.Show("Setup error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void CopyIcon()
        {
            try
            {
                string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string sourceIcon = Path.Combine(exeDir, "PdfCompressor.ico");
                string destIcon = Path.Combine(DataDir, "PdfCompressor.ico");

                if (File.Exists(sourceIcon))
                {
                    File.Copy(sourceIcon, destIcon, true);
                }
            }
            catch (Exception ex)
            {
                CrashLogger.Log(ex);
                MessageBox.Show("Icon copy error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}