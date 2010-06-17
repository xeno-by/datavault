using System.Linq;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.UI.Api.ApiExtensions;

namespace DataVault.UI.Api.UIContext
{
    public abstract class ValueRelatedContextBoundCommand : ContextBoundCommand
    {
        protected ValueRelatedContextBoundCommand(DataVaultUIContext context) 
            : base(context) 
        {
        }

        protected ListViewItem RefreshListAndThenSelect(IValue value)
        {
            Tree.SelectedNode = Tree.Nodes[0].SelectNode(Branch.VPath);
            Ctx.RebuildListItems();

            if (value != null)
            {
                var lvi = List.Items.Cast<ListViewItem>().Single(lvi2 => lvi2.Text == value.Name);
                lvi.Selected = true;
                return lvi;
            }
            else
            {
                if (List.Items.Count > 0) List.Items[0].Selected = true;
                return null;
            }
        }
    }
}