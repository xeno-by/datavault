using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class PasteCommand : ContextBoundCommand
    {
        private ICommand Impl { get; set; }

        public PasteCommand(DataVaultUIContext context)
            : base(context)
        {
            if (Tree.Focused)
            {
                Impl = new BranchPasteCommand(context);
            }
            else if (List.Focused)
            {
                Impl = new ValuePasteCommand(context);
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