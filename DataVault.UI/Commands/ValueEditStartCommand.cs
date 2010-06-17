using System;
using System.Windows.Forms;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Impl.Controls;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class ValueEditStartCommand : ContextBoundCommand
    {
        public ValueEditStartCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Value != null;
        }

        public override void DoImpl()
        {
            var editForm = new DefaultEditValueForm(Ctx, Value);
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