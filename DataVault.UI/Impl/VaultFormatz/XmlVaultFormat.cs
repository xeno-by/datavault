using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using DataVault.Core.Api;
using DataVault.UI.Api.VaultFormatz;
using DataVault.UI.Properties;

namespace DataVault.UI.Impl.VaultFormatz
{
    [VaultFormat("xml"), VaultFormatLoc(typeof(Resources), "VaultFormat_Xml_DialogTitle", "VaultFormat_Xml_TabTitle")]
    public class XmlVaultFormat : FileBasedVaultFormat
    {
        protected override bool AcceptVaultCore(string uri)
        {
            try
            {
                new XmlDocument().Load(uri);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override IVault OpenVaultCore(String uri)
        {
            try
            {
                return VaultApi.OpenXml(uri);
            }
            catch (Exception)
            {
                MessageBox.Show(Resources.Validation_InvalidXmlVault, Resources.Validation_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        protected override bool VaultExists(String uri)
        {
            return File.Exists(uri);
        }
    }
}