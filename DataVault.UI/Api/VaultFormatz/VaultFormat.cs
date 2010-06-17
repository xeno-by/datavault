using System;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Reflection;
using DataVault.UI.Api.VaultFormatz.Dialogs;

namespace DataVault.UI.Api.VaultFormatz
{
    public abstract class VaultFormat : IVaultFormat
    {
        public String Name { get { return this.GetType().Attr<VaultFormatAttribute>().Name; } }

        public String LocDialogTitle(VaultAction action)
        {
            return this.GetType().Attr<VaultFormatLocAttribute>().DialogTitle(action);
        }
        public String LocTabTitle(VaultAction action)
        {
            return this.GetType().Attr<VaultFormatLocAttribute>().TabTitle(action);
        }

        public void InjectIntoVaultDialog(IVaultDialog dialog)
        {
            var action = dialog.Action;
            var virtualTab = dialog.AddTab(LocTabTitle(action), LocDialogTitle(action));
            BuildDialogControls(dialog, virtualTab);
        }

        public abstract bool AcceptCore(string uri);
        public abstract IVault OpenCore(String uri);
        public abstract void BuildDialogControls(IVaultDialog dialog, IVaultDialogTab tab);
    }
}