namespace DataVault.UI.Api.Commands
{
    public interface ICommand
    {
        bool CanDo();
        void Do();
        void Undo();
    }
}