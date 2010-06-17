using System.Linq;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Commands;
using DataVault.Core.Helpers;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public class BranchMoveDownCommand : ContextBoundCommand
    {
        private ICommand AntiCommand { get; set; }

        public BranchMoveDownCommand(DataVaultUIContext context)
            : base(context)
        {
            AntiCommand = new BranchMoveUpCommand(context, this);
        }

        public BranchMoveDownCommand(DataVaultUIContext context, BranchMoveUpCommand antiCommand)
            : base(context)
        {
            AntiCommand = antiCommand;
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null && Branch.Parent != null
                   && Branch.Parent.GetBranches().Last() != Branch;
        }

        public override void DoImpl()
        {
            var p = Branch.Parent;
            var i = p.GetBranches().ToList().IndexOf(Branch);
            var list = p.GetBranches().ToArray();

            list.ForEach(n => n.Delete());
            list.Take(i).ForEach(n => p.AttachBranch(n));
            p.AttachBranch(list.ElementAt(i + 1));
            p.AttachBranch(list.ElementAt(i));
            list.Skip(i + 2).ForEach(n => p.AttachBranch(n));

            var treeNode = Tree.Nodes[0].SelectNode(Branch.VPath);
            var parentNode = treeNode.Parent;
            treeNode.Remove();
            parentNode.Nodes.Insert(i + 1, treeNode); // i+1 has become i after deletion
            Tree.SelectedNode = treeNode;
        }

        public override void UndoImpl()
        {
            AntiCommand.Do();
        }
    }
}