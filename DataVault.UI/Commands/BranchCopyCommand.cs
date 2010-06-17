using System;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class BranchCopyCommand : ContextBoundCommand
    {
        public BranchCopyCommand(DataVaultUIContext context) 
            : base(context) 
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null && Branch.Parent != null;
        }

        public override void DoImpl()
        {
            BranchInClipboard = Branch;
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}