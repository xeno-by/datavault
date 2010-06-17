using System;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Collections;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.Commands.WithExecutor;
using DataVault.UI.Api.VaultViewz;
using DataVault.UI.Impl.Helpers;
using DataVault.UI.Properties;
using DataVault.UI.Utils.CustomMessageBoxes;

namespace DataVault.UI.Api.UIContext
{
    public abstract class ContextBoundCommand : ICommand
    {
        public DataVaultUIContext Ctx { get; private set; }
        public IVault Vault { get { return Ctx.Vault; } }
        public StackSlim<IVaultView> Views { get { return Ctx.Views; } }
        public DataVaultEditor DataVaultEditor { get { return Ctx.DataVaultEditor; } }
        public TreeView Tree { get { return DataVaultEditor._tree; } }
        public ListView List { get { return DataVaultEditor._list; } }
        public IBranch Branch { get; private set; }
        public IValue Value { get; private set; }
        public IBranch BranchInClipboard { get { return Ctx.BranchInClipboard; } set { Ctx.BranchInClipboard = value;} }
        public IValue ValueInClipboard { get { return Ctx.ValueInClipboard; } set { Ctx.ValueInClipboard = value; } }

        protected ContextBoundCommand(DataVaultUIContext context) 
        {
            Ctx = context;

            DataVaultEditor.ThreadSafeInvoke(() =>
            {
                Branch = Tree.SelectedNode == null ? null : (IBranch)Tree.SelectedNode.Tag;
                Value = List.SelectedItems.Count == 0 ? null : (IValue)List.SelectedItems[0].Tag;
            });
        }

        public bool CanDo()
        {
            return DataVaultEditor.ThreadSafeInvoke(() => CanDoImpl());
        }

        public void Do()
        {
            DataVaultEditor.ThreadSafeInvoke(DoImpl);
        }

        public void Undo()
        {
            DataVaultEditor.ThreadSafeInvoke(UndoImpl);
        }

        public virtual bool CanDoImpl()
        {
            return Ctx.CommandsAllowed;
        }

        public abstract void DoImpl();
        public abstract void UndoImpl();

        protected void MakeSureItsSafeToCloseTheVault()
        {
            if (Ctx.IsDirty)
            {
                var dlgResult = CustomMessageBox.Show(
                    Resources.CloseVaultWhenDirty_Message,
                    Resources.CloseVaultWhenDirty_Title,
                    Resources.CloseVaultWhenDirty_Buttons,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                // save & exit
                if (dlgResult == 1)
                {
                    Vault.Save();
                }
                // don't save
                else if (dlgResult == 2)
                {
                    // just proceed
                }
                // cancel
                else
                {
                    (dlgResult == 3).AssertTrue();
                    throw new CommandExecutionCancelledException();
                }
            }
        }
    }
}