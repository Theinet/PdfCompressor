using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using Syncfusion.Licensing;

namespace PdfCompressor
{
    public partial class App : Application
    {
        private readonly string DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PdfCompressor");

        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            SetupApplicationFiles();

            string licenseKey = LoadLicenseKey();
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                MessageBox.Show(
                    "Syncfusion license key is missing.\n\nconfig.json must exist at:\n" + Path.Combine(DataDir, "config.json"),
                    "Missing License",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Shutdown();
                return;
            }

            SyncfusionLicenseProvider.RegisterLicense(licenseKey);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            base.OnStartup(e);
        }

        private void SetupApplicationFiles()
        {
            try
            {
                if (!Directory.Exists(DataDir))
                    Directory.CreateDirectory(DataDir);

                string configSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                string configTarget = Path.Combine(DataDir, "config.json");

                if (!File.Exists(configTarget) && File.Exists(configSource))
                    File.Copy(configSource, configTarget);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying config.json: {ex.Message}", "Setup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private string LoadLicenseKey()
        {
            try
            {
                string configPath = Path.Combine(DataDir, "config.json");
                if (!File.Exists(configPath))
                    return null;

                string json = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return config != null && config.TryGetValue("LicenseKey", out string key) ? key : null;
            }
            catch
            {
                return null;
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogCrash(e.Exception);
            MessageBox.Show($"Unexpected error:\n{e.Exception.Message}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                LogCrash(ex);
        }

        private void LogCrash(Exception ex)
        {
            try
            {
                string path = Path.Combine(DataDir, "crash.log");
                File.AppendAllText(path, $"[{DateTime.Now}] {ex}\n\n");
            }
            catch
            {
            }
        }
    }
}
