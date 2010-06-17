using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class BranchDeleteCommand : ContextBoundCommand
    {
        public BranchDeleteCommand(DataVaultUIContext context)
            : base(context) 
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null && Branch.Parent != null;
        }

        public override void DoImpl()
        {
            Branch.Delete();

            Tree.SelectedNode = Tree.Nodes[0].SelectNode(Branch.VPath);
            var p = Tree.SelectedNode.Parent;
            Tree.SelectedNode.Remove();
            Tree.SelectedNode = p;
        }

        public override void UndoImpl()
        {
            Branch.Parent.AttachBranch(Branch);

            var p = Tree.Nodes[0].SelectNode(Branch.VPath.Parent).AssertNotNull();
            Tree.SelectedNode = Ctx.CreateTreeNodesRecursive(p, Branch);
        }
    }
}