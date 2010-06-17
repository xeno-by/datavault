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
    public class BranchPasteCommand : ContextBoundCommand
    {
        private IBranch CreatedBranch { get; set; }

        public BranchPasteCommand(DataVaultUIContext context)
            : base(context) 
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && BranchInClipboard != null && Branch != null;
        }

        public override void DoImpl()
        {
            Func<int, String> namegen = i => i == 1 ? BranchInClipboard.Name : String.Format(
                Resources.New_BranchDefaultPastedName, BranchInClipboard.Name, i - 1);
            var lastUsedIndex = 1.Seq(i => i + 1, i => Branch.GetBranches().Any(b => b.Name == namegen(i))).LastOrDefault();
            var unusedName = namegen(lastUsedIndex + 1);

#if DOWNGRADE_STRUCTURE_VERSION_TO_REV299
            // one can't use SetDefault() here, because the default value will be copied
            // from the clipboard rather than created manually
            CreatedBranch = Branch.CreateBranch(unusedName).SetId(Guid.NewGuid());
#else
            // upd. safe to use SetDefault2 because branches no longer have default values
            CreatedBranch = Branch.CreateBranch(unusedName).SetDefault2();
#endif

            // the line below works fine only because the branch in clipboard is actually a clone that is disconnected from reality. 
            // only imagine what if you press Ctrl+C on a branch w/o sub-branches for simplicity and then sequentially press Ctrl+V
            // 1st paste will actually work fine and create a sub-branch w/o no sub-branches and the same values as the source branch had
            // 2nd paste will use THE CHANGED source branch and will created a sub-sub-branch with a single sub-branch (which is counter-intuitive)

            var branches = BranchInClipboard.GetBranchesRecursive();
            branches.ForEach(b => CreatedBranch.GetOrCreateBranch(b.VPath - BranchInClipboard.VPath));
            BranchInClipboard.GetValuesRecursive().ForEach(v => CreatedBranch
                .CreateValue(v.VPath - BranchInClipboard.VPath, () => v.ContentStream)
                .SetDefault2().SetTypeToken2(v.GetTypeToken2()));

            Tree.SelectedNode = Tree.Nodes[0].SelectNode(Branch.VPath);
            Tree.SelectedNode = Ctx.CreateTreeNodesRecursive(Tree.SelectedNode, CreatedBranch);
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