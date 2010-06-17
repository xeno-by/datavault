using System;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class BranchRenameStartCommand : ContextBoundCommand
    {
        public BranchRenameStartCommand(DataVaultUIContext context)
            : base(context) 
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null && Branch.Parent != null;
        }

        public override void DoImpl()
        {
            Tree.SelectedNode.BeginEdit();
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}