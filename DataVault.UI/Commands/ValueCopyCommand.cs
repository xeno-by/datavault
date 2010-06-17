using System;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class ValueCopyCommand : ContextBoundCommand
    {
        public ValueCopyCommand(DataVaultUIContext context)
            : base(context) 
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Value != null;
        }

        public override void DoImpl()
        {
            ValueInClipboard = Value;
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}