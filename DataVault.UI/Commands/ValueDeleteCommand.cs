using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class ValueDeleteCommand : ValueRelatedContextBoundCommand
    {
        public ValueDeleteCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Value != null && Value.Name != "default";
        }

        public override void DoImpl()
        {
            Value.Delete();
            RefreshListAndThenSelect(null);
        }

        public override void UndoImpl()
        {
            Value.Parent.AttachValue(Value);
            RefreshListAndThenSelect(Value);
        }
    }
}