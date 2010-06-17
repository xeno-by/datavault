using System;
using System.IO;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Impl.Zip.ZipLib.Exceptions;
using DataVault.UI.Api.VaultFormatz;
using DataVault.UI.Properties;
using DataVault.Core.Helpers;

namespace DataVault.UI.Impl.VaultFormatz
{
    [VaultFormat("zip"), VaultFormatLoc(typeof(Resources), "VaultFormat_Zip_DialogTitle", "VaultFormat_Zip_TabTitle")]
    public class ZipVaultFormat : FileBasedVaultFormat
    {
        protected override bool AcceptVaultCore(string uri)
        {
            try
            {
                var t_zipFile = typeof(VaultApi).Assembly.GetType("DataVault.Core.Impl.Zip.ZipLib.ZipFile");
                var zipFile = Activator.CreateInstance(t_zipFile, uri);
                zipFile.AssertCast<IDisposable>().Dispose();

                return true;
            }
            catch (ZipException)
            {
                return false;
            }
        }

        protected override IVault OpenVaultCore(String uri)
        {
            try
            {
                return VaultApi.OpenZip(uri);
            }
            catch (Exception)
            {
                MessageBox.Show(Resources.Validation_InvalidZipVault, Resources.Validation_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        protected override bool VaultExists(String uri)
        {
            return File.Exists(uri);
        }
    }
}