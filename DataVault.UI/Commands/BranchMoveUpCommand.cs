using System.Linq;
using DataVault.Core.Helpers;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class BranchMoveUpCommand : ContextBoundCommand
    {
        private ICommand AntiCommand { get; set; }

        public BranchMoveUpCommand(DataVaultUIContext context)
            : base(context)
        {
            AntiCommand = new BranchMoveDownCommand(context, this);
        }

        public BranchMoveUpCommand(DataVaultUIContext context, BranchMoveDownCommand antiCommand)
            : base(context)
        {
            AntiCommand = antiCommand;
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null && Branch.Parent != null
                && Branch.Parent.GetBranches().First() != Branch;
        }

        public override void DoImpl()
        {
            var p = Branch.Parent;
            var i = p.GetBranches().ToList().IndexOf(Branch);
            var list = p.GetBranches().ToArray();

            list.ForEach(n => n.Delete());
            list.Take(i - 1).ForEach(n => p.AttachBranch(n));
            p.AttachBranch(list.ElementAt(i));
            p.AttachBranch(list.ElementAt(i - 1));
            list.Skip(i + 1).ForEach(n => p.AttachBranch(n));

            var treeNode = Tree.Nodes[0].SelectNode(Branch.VPath);
            var parentNode = treeNode.Parent;
            treeNode.Remove();
            parentNode.Nodes.Insert(i - 1, treeNode);
            Tree.SelectedNode = treeNode;
        }

        public override void UndoImpl()
        {
            AntiCommand.Do();
        }
    }
}