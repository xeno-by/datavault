using System;

namespace DataVault.UI.Api.Commands.WithExecutor
{
    public class CommandExecutedEventArgs : EventArgs
    {
        public ICommand Command { get; private set; }

        public CommandExecutedEventArgs(ICommand command)
        {
            Command = command;
        }
    }
}