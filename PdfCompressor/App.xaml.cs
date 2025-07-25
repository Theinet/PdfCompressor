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
                CustomMessageBox.Show("Initialization error:\n" + ex.Message, "Error");
            }

            this.DispatcherUnhandledException += (sender, args) =>
            {
                CrashLogger.Log(args.Exception);
                CustomMessageBox.Show("An unexpected error occurred:\n" + args.Exception.Message, "Error");
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    CrashLogger.Log(ex);
                    CustomMessageBox.Show("A fatal error occurred:\n" + ex.Message, "Fatal Error");
                }
            };
        }
    }
}