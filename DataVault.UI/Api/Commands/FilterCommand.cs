using System;
using DataVault.UI.Api.Commands.WithHistory;

namespace DataVault.UI.Api.Commands
{
    [GhostableInHistory]
    public class FilterCommand : ICommand
    {
        private Func<bool> Predicate { get; set; }

        public FilterCommand(Func<bool> predicate) 
        {
            Predicate = predicate;
        }

        public bool CanDo()
        {
            return Predicate();
        }

        public void Do()
        {
        }

        public void Undo()
        {
            throw new NotSupportedException();
        }
    }
}