using System;
using System.Windows.Forms;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Commands.WithExecutor;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.VaultFormatz;
using DataVault.UI.Api.VaultFormatz.Dialogs;

namespace DataVault.UI.Commands
{
    [ResetsHistory, GhostableInHistory]
    public class VaultOpenCommand : ContextBoundCommand
    {
        public VaultOpenCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override void DoImpl()
        {
            // check for modifications of current vault
            MakeSureItsSafeToCloseTheVault();

            // open a new vault
            var vaultDialog = new VaultDialogForm(VaultAction.Open);
            if (vaultDialog.ShowDialog(DataVaultEditor) == DialogResult.OK)
            {
                var vault = vaultDialog.SelectedVault.AssertNotNull();
                Ctx.SetVault(vault, true);
            }
            else
            {
                throw new CommandExecutionCancelledException();
            }

            // load previously opened views for the vault loaded
            Ctx.Execute(new CtxInitializeViews(Ctx));
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}