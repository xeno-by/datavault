using System;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;

namespace DataVault.UI.Commands
{
    public class BranchNewCommand : ContextBoundCommand
    {
        private IBranch CreatedBranch { get; set; }

        public BranchNewCommand(DataVaultUIContext context)
            : base(context) 
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null;
        }

        public override void DoImpl()
        {
            Func<int, String> namegen = i => String.Format(Resources.New_BranchDefaultName, i);
            var lastUsedIndex = 1.Seq(i => i + 1, i => Branch.GetBranches().Any(b => b.Name == namegen(i))).LastOrDefault();
            var unusedName = namegen(lastUsedIndex + 1);

            CreatedBranch = Branch.CreateBranch(unusedName).SetDefault2();

            Tree.SelectedNode = Tree.Nodes[0].SelectNode(Branch.VPath);
            var tn = CreatedBranch.AsUIElement();
            Tree.SelectedNode.Nodes.Add(tn);

            Tree.SelectedNode = tn;
            tn.BeginEdit();
        }

        public override void UndoImpl()
        {
            CreatedBranch.Delete();

            var tn = Tree.Nodes[0].SelectNode(CreatedBranch.VPath).AssertNotNull();
            Tree.SelectedNode = tn.Parent;
            tn.Remove();
        }
    }
}
