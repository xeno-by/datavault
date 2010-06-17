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
    [ResetsHistory, GhostableInHistory]
    public class VaultImportCommand : ContextBoundCommand
    {
        public VaultImportCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return Ctx.Vault != null;
        }

        public override void DoImpl()
        {
            var vaultDialog = new VaultDialogForm(VaultAction.Import);
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
                        if (MessageBox.Show(Resources.Import_Precaution_Message, Resources.Import_Precaution_Title,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            // we need to discard views before import
                            // to avoid possible side-effects they introduce
                            Views.ForEach(v => { v.Discard(); Views.Pop(); });

                            var @this = Ctx.Vault;
                            @this.Root.Delete();
                            @this.ImportFrom(vault);
                            @this.Save();

                            Ctx.SetVault(@this, true);
                        }
                        else
                        {
                            throw new CommandExecutionCancelledException();
                        }
                    }
                }
            }
            else
            {
                throw new CommandExecutionCancelledException();
            }
        }

        // todo. actually, it'd be quite easy to implement undo for this command

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}