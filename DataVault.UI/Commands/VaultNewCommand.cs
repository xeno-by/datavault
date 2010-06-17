using System;
using System.Windows.Forms;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Commands.WithExecutor;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.VaultFormatz;
using DataVault.UI.Api.VaultFormatz.Dialogs;
using DataVault.UI.Properties;

namespace DataVault.UI.Commands
{
    [ResetsHistory, GhostableInHistory]
    public class VaultNewCommand : ContextBoundCommand
    {
        public VaultNewCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override void DoImpl()
        {
            MakeSureItsSafeToCloseTheVault();
            
            var vaultDialog = new VaultDialogForm(VaultAction.Create);
            if (vaultDialog.ShowDialog(DataVaultEditor) == DialogResult.OK)
            {
                var vault = vaultDialog.SelectedVault.AssertNotNull();
                Ctx.SetVault(vault, true);
            }
            else
            {
                throw new CommandExecutionCancelledException();
            }
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}