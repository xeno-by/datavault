using DataVault.UI.Api.Commands.WithHistory;

namespace DataVault.UI.Api.Commands
{
    [GhostableInHistory]
    public class EmptyCommand : ICommand
    {
        public bool CanDo()
        {
            return false;
        }

        public void Do()
        {
        }

        public void Undo()
        {
        }
    }
}