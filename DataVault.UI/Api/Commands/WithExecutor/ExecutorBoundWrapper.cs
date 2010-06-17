using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.UI.Api.Commands.WithExecutor
{
    public class ExecutorBoundWrapper<T> : ICommand
        where T : class, ICommandExecutor
    {
        private T Executor { get; set; }
        private ICommand Command { get; set; }

        public ExecutorBoundWrapper(T executor, ICommand command)
        {
            Executor = executor.AssertNotNull();
            Command = command.AssertNotNull();
        }

        public bool CanDo()
        {
            return Command.CanDo();
        }

        public void Do()
        {
            Executor.Execute(Command);
        }

        public void Undo()
        {
            Executor.Unexecute(Command);
        }
    }
}