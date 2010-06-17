using DataVault.Core.Api;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.ComponentModel
{
    public class BranchEditor : ElementEditor
    {
        protected override bool ApproveSelection(DataVaultUIContext context, IElement el)
        {
            return el is IBranch && el.Parent != null;
        }
    }
}