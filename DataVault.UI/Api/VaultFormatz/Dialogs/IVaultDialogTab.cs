using System;
using System.Windows.Forms;

namespace DataVault.UI.Api.VaultFormatz.Dialogs
{
    public interface IVaultDialogTab
    {
        Guid Id { get; }
        TabPage TabPage { get; }

        String ShortTitle { get; set; }
        String FullTitle { get; set; }
    }
}