using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataVault.Core.Helpers;
using DataVault.UI.Api.Commands.WithHistory;

namespace DataVault.UI.Api.Commands
{
    public class CompositeCommand : ICommand
    {
        public IEnumerable<ICommand> Commands { get; private set;}

        public CompositeCommand() 
            : this(Enumerable.Empty<ICommand>())
        {
        }

        public CompositeCommand(IEnumerable<ICommand> commands)
        {
            Commands = new ReadOnlyCollection<ICommand>(commands.ToList());
        }

        public bool CanDo()
        {
            return Commands.Count() != 0 && Commands.All(c => c.CanDo());
        }

        public void Do()
        {
            Commands.ForEach(c => c.Do());
        }

        public void Undo()
        {
            Commands.Reverse().ForEach(c => 
            {
                if (!c.GetType().IsDefined(typeof(GhostableInHistoryAttribute), true))
                    c.Undo();
            });
        }
    }
}