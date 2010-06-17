using System;

namespace DataVault.UI.Api.Commands.WithExecutor
{
    public class CommandExecutingEventArgs : EventArgs
    {
        public ICommand Command { get; private set; }
        public bool Cancel { get; set; }

        public CommandExecutingEventArgs(ICommand command)
        {
            Command = command;
        }
    }
}