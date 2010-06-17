using DataVault.UI.Api.Commands;

namespace DataVault.UI.Api.Commands.WithHistory
{
    [GhostableInHistory]
    public class RedoCommand : ICommand
    {
        public CommandExecutorWithHistory Executor { get; private set; }

        public RedoCommand(CommandExecutorWithHistory executor)
        {
            Executor = executor;
        }

        public bool CanDo()
        {
            return Executor.CanRedo();
        }

        public void Do()
        {
            Executor.Redo();
        }

        public void Undo()
        {
            Executor.Undo();
        }
    }
}