using System;
using System.Drawing;
using System.Linq;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Exceptions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;
using DataVault.Core.Helpers;

namespace DataVault.UI.Commands
{
    public class BranchRenameFinishCommand : ContextBoundCommand
    {
        private String NewName { get; set; }
        private String OldName { get; set; }

        public BranchRenameFinishCommand(DataVaultUIContext context, String newName)
            : base(context)
        {
            NewName = newName;
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null && Branch.Parent != null && NewName != Branch.Name;
        }

        public override void DoImpl()
        {
            if (Branch.Parent.GetBranches().Any(b => b.Name == NewName))
                throw new ValidationException(Resources.Validation_DuplicateBranchName, NewName);

            if (NewName.IsNullOrEmpty())
                throw new ValidationException(Resources.Validation_InvalidName, NewName);

            try
            {
                var tn = Tree.Nodes[0].SelectNode(Branch.VPath).AssertNotNull();
                tn.ForeColor = NewName.StartsWith("$") ? Color.LightGray : Color.Black;

                OldName = Branch.Name;
                Branch.Rename(NewName);
            }
            catch(ArgumentException)
            {
                throw new ValidationException(Resources.Validation_InvalidName, NewName);
            }
        }

        public override void UndoImpl()
        {
            var newvpath = Branch.VPath;
            Branch.Rename(OldName.AssertNotNull());

            var tn = Tree.Nodes[0].SelectNode(newvpath).AssertNotNull();
            tn.Text = OldName;
            tn.ForeColor = tn.Text.StartsWith("$") ? Color.LightGray : Color.Black;
            Tree.SelectedNode = tn;
        }
    }
}