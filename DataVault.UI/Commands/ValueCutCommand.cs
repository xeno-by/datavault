using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class ValueCutCommand : CompositeCommand
    {
        public ValueCutCommand(DataVaultUIContext context)
            : base(new ICommand[]{new ValueCopyCommand(context), new ValueDeleteCommand(context)})
        {
        }
    }
}