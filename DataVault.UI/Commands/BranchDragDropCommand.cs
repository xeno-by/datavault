using System;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;

namespace DataVault.UI.Commands
{
    public class BranchDragDropCommand : ContextBoundCommand
    {
        private IBranch DraggedBranch { get; set; }
        private IBranch OldParent { get; set; }
        private String OldName { get; set; }

        public BranchDragDropCommand(DataVaultUIContext context, IBranch draggedBranch)
            : base(context)
        {
            DraggedBranch = draggedBranch;
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null && DraggedBranch != null &&
                DraggedBranch.Parent != null && DraggedBranch != Branch && DraggedBranch != Branch.Parent;
        }

        public override void DoImpl()
        {
            var draggedNode = Tree.Nodes[0].SelectNode(DraggedBranch.VPath);
            OldParent = DraggedBranch.Parent;
            OldName = DraggedBranch.Name;

            Func<int, String> namegen = i => i == 1 ? DraggedBranch.Name : String.Format(
                Resources.New_BranchDefaultPastedName, DraggedBranch.Name, i - 1);
            var lastUsedIndex = 1.Seq(i => i + 1, i => Branch.GetBranches().Any(b => b.Name == namegen(i))).LastOrDefault();
            DraggedBranch.Rename(namegen(lastUsedIndex + 1));
            draggedNode.Text = DraggedBranch.Name;

            DraggedBranch.Delete();
            Branch.AttachBranch(DraggedBranch);

            draggedNode.Remove();
            Tree.SelectedNode = Tree.Nodes[0].SelectNode(Branch.VPath);
            Tree.SelectedNode.Nodes.Add(draggedNode);
            Tree.SelectedNode = draggedNode;
        }

        public override void UndoImpl()
        {
            var draggedNode = Tree.Nodes[0].SelectNode(DraggedBranch.VPath);
            draggedNode.Text = OldName;

            DraggedBranch.Delete();
            DraggedBranch.Rename(OldName);
            OldParent.AttachBranch(DraggedBranch);

            var oldParentNode = Tree.Nodes[0].SelectNode(OldParent.VPath);
            draggedNode.Remove();
            oldParentNode.Nodes.Add(draggedNode);
            Tree.SelectedNode = draggedNode;
        }
    }
}