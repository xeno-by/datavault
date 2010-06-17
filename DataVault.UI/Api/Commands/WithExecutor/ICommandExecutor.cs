using System;

namespace DataVault.UI.Api.Commands.WithExecutor
{
    public interface ICommandExecutor
    {
        bool Execute(ICommand command);
        bool Unexecute(ICommand command);

        bool InExecute { get; }
        bool InUnexecute { get; }
        ICommand ActiveCommand { get; }

        event EventHandler<CommandExecutingEventArgs> CommandExecuting;
        event EventHandler<CommandExecutedEventArgs> CommandExecutionCancelled;
        event EventHandler<CommandExecutedEventArgs> CommandExecuted;
        event EventHandler<CommandExecutedEventArgs> CommandUnexecutionCancelled;
        event EventHandler<CommandExecutingEventArgs> CommandUnexecuting;
        event EventHandler<CommandExecutedEventArgs> CommandUnexecuted;
    }
}