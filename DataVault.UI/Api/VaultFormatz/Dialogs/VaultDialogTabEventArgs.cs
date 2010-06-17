using System;

namespace DataVault.UI.Api.VaultFormatz.Dialogs
{
    public class VaultDialogTabEventArgs : EventArgs
    {
        public IVaultDialogTab Tab { get; private set; }

        public VaultDialogTabEventArgs(IVaultDialogTab tab)
        {
            Tab = tab;
        }
    }
}