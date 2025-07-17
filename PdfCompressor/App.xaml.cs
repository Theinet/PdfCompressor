using System;
using System.Windows;
using PdfCompressor.Helpers;

namespace PdfCompressor
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                FileHelper.SetupAppDataDirectory();
            }
            catch (Exception ex)
            {
                CrashLogger.Log(ex);
                MessageBox.Show("Initialization error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.DispatcherUnhandledException += (sender, args) =>
            {
                CrashLogger.Log(args.Exception);
                MessageBox.Show("An unexpected error occurred:\n" + args.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    CrashLogger.Log(ex);
                    MessageBox.Show("A fatal error occurred:\n" + ex.Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
    }
}
