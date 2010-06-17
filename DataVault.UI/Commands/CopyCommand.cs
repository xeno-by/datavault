using DataVault.UI.Api.Commands;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class CopyCommand : ContextBoundCommand
    {
        private ICommand Impl { get; set; }

        public CopyCommand(DataVaultUIContext context)
            : base(context)
        {
            if (Tree.Focused)
            {
                Impl = new BranchCopyCommand(context);
            }
            else if (List.Focused)
            {
                Impl = new ValueCopyCommand(context);
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