using System;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class ValueRenameStartCommand : ContextBoundCommand
    {
        public ValueRenameStartCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Value != null && Value.Name != "default";
        }

        public override void DoImpl()
        {
            List.SelectedItems[0].BeginEdit();
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}