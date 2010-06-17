using DataVault.UI.Api.Commands;

namespace DataVault.UI.Api.Commands.WithHistory
{
    [GhostableInHistory]
    public class UndoCommand : ICommand
    {
        public CommandExecutorWithHistory Executor { get; private set; }

        public UndoCommand(CommandExecutorWithHistory executor) 
        {
            Executor = executor;
        }

        public bool CanDo()
        {
            return Executor.CanUndo();
        }

        public void Do()
        {
            Executor.Undo();
        }

        public void Undo()
        {
            Executor.Redo();
        }
    }
}