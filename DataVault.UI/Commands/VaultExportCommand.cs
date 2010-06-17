using System;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Commands.WithExecutor;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.Exceptions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.VaultFormatz;
using DataVault.UI.Api.VaultFormatz.Dialogs;
using DataVault.UI.Properties;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class VaultExportCommand : ContextBoundCommand
    {
        public VaultExportCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Vault != null;
        }

        public override void DoImpl()
        {
            var vaultDialog = new VaultDialogForm(VaultAction.Export);
            if (vaultDialog.ShowDialog(DataVaultEditor) == DialogResult.OK)
            {
                using (var vault = vaultDialog.SelectedVault.AssertNotNull())
                {
                    if (Vault.Uri == vault.Uri)
                    {
                        throw new ValidationException(Resources.Validation_SourceAndTargetOfExportImportAreTheSame);
                    }
                    else
                    {
                        vault.ImportFrom(vault, CollisionHandling.Overwrite);
                        vault.Save();
                    }
                }
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