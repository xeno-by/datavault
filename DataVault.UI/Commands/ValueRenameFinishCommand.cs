using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Exceptions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;
using DataVault.Core.Helpers;

namespace DataVault.UI.Commands
{
    public class ValueRenameFinishCommand : ContextBoundCommand
    {
        private String NewName { get; set; }
        private String OldName { get; set; }

        public ValueRenameFinishCommand(DataVaultUIContext context, String newName)
            : base(context)
        {
            NewName = newName;
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Value != null && NewName != Value.Name;
        }

        public override void DoImpl()
        {
            if (Value.Parent.GetValues().Any(v => v.Name == NewName))
                throw new ValidationException(Resources.Validation_DuplicateValueName, NewName);

            if (NewName.IsNullOrEmpty())
                throw new ValidationException(Resources.Validation_InvalidName, NewName);

            try
            {
                var lvi = List.Items.Cast<ListViewItem>().Single(item => item.Text == Value.Name);
                lvi.ForeColor = NewName.StartsWith("$") ? Color.LightGray : Color.Black;

                OldName = Value.Name;
                Value.Rename(NewName);
            }
            catch (ArgumentException)
            {
                throw new ValidationException(Resources.Validation_InvalidName, NewName);
            }
        }

        public override void UndoImpl()
        {
            Value.Rename(OldName.AssertNotNull());

            var host = Tree.Nodes[0].SelectNode(Value.Parent.VPath);
            if (Tree.SelectedNode != host)
            {
                // reselect host tree node if necessary, so we don't have probs with list items
                // note: if the node was reselected, we don't have to rename the LVI, since the list view is already sync with datasource
                Tree.SelectedNode = host;
            }
            else
            {
                var lvi = List.Items.Cast<ListViewItem>().Single(item => item.Text == NewName);
                lvi.Text = OldName;
                lvi.ForeColor = lvi.Text.StartsWith("$") ? Color.LightGray : Color.Black;
            }

            List.Items.Cast<ListViewItem>().Single(item => item.Text == Value.Name).Selected = true;
        }
    }
}