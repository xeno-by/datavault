using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using DataVault.Core.Api;

namespace DataVault.UI.Api.VaultFormatz.Dialogs
{
    public interface IVaultDialog
    {
        VaultAction Action { get; }
        IVault SelectedVault { get; }
        void EndDialog();
        void EndDialog(IVault vault);

        ReadOnlyCollection<IVaultDialogTab> Tabs { get; }
        IVaultDialogTab AddTab(String shortTitle, String fullTitle);

        IVaultDialogTab ActiveTab { get; }
        event EventHandler<VaultDialogTabEventArgs> TabActivated;
        event EventHandler<VaultDialogTabEventArgs> TabDeactivated;

        Button OkButton { get; }
    }
}
