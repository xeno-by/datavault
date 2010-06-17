using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.UI.Api;
using DataVault.UI.Api.VaultFormatz;
using DataVault.UI.Commands;
using DataVault.Core.Helpers;
using DataVault.UI.Properties;
using System.Linq;

namespace DataVault.UI
{
    // todo. when a binary file is opened by another app 
    // we should asynchronously wait for that app to finish and then check it for modifications
    // be it modified, we should display user a dialog with a suggestion to repack it back

    internal static class Program
    {
        [STAThread]
        public static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // this, but not Application.CurrentCulture makes resource manager to load russian strings
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");

            // Open the vault at %1 (damn, why didn't I think about this stuff earlier?!)
            var initialUri = args.SingleOrDefaultDontCrash();
            var mainForm = new DataVaultEditorForm(initialUri);
            Application.Run(mainForm);
        }
    }
}
