using System.Globalization;
using System.Resources;
using PdfCompressor.Properties;

namespace PdfCompressor
{
    /// <summary>
    /// Provides access to localized strings from the .resx resource files.
    /// Supports culture switching at runtime.
    /// </summary>
    public static class LocalizationManager
    {
        private static readonly ResourceManager resourceManager = Resources.ResourceManager;

        /// <summary>
        /// Sets the current UI and thread culture.
        /// </summary>
        public static void SetCulture(string cultureName)
        {
            CultureInfo culture = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
        }

        /// <summary>
        /// Retrieves a localized string for the given resource key.
        /// Returns [key] if not found.
        /// </summary>
        public static string GetString(string key)
        {
            try
            {
                string value = resourceManager.GetString(key, CultureInfo.CurrentUICulture);
                return value ?? $"[{key}]";
            }
            catch
            {
                return $"[{key}]";
            }
        }
    }
}
