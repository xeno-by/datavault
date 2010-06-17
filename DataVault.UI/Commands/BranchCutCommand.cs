using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class BranchCutCommand : CompositeCommand
    {
        public BranchCutCommand(DataVaultUIContext context)
            : base(new ICommand[]{new BranchCopyCommand(context), new BranchDeleteCommand(context)})
        {
        }
    }
}