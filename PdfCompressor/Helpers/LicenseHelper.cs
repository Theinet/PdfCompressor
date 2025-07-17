using Syncfusion.Licensing;
using PdfCompressor.License;    

namespace PdfCompressor.Helpers
{
    public static class LicenseHelper
    {
        public static void ApplySyncfusionLicense()
        {
            SyncfusionLicenseProvider.RegisterLicense(LicenseManager.GetLicense());
        }
    }
}
