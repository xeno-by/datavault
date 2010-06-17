using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class CutCommand : ContextBoundCommand
    {
        private ICommand Impl { get; set; }

        public CutCommand(DataVaultUIContext context)
            : base(context)
        {
            if (Tree.Focused)
            {
                Impl = new BranchCutCommand(context);
            }
            else if (List.Focused)
            {
                Impl = new ValueCutCommand(context);
            }
            else
            {
                Impl = new EmptyCommand();
            }
        }

        public override bool CanDoImpl()
        {
            return Impl.CanDo();
        }

        public override void DoImpl()
        {
            Impl.Do();
        }

        public override void UndoImpl()
        {
            Impl.Undo();
        }
    }
}