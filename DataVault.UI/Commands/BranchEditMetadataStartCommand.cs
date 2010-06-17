using System;
using System.Windows.Forms;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Impl.Controls;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class BranchEditMetadataStartCommand : ContextBoundCommand
    {
        public BranchEditMetadataStartCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null;
        }

        public override void DoImpl()
        {
            var editForm = new DefaultEditMetadataForm(Ctx, Branch);
            editForm.StartPosition = FormStartPosition.CenterParent;

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                var cmd = editForm.IssueApplyChangesCommand();
                if (cmd != null)
                {
                    Ctx.Execute(cmd);
                }
            }
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}