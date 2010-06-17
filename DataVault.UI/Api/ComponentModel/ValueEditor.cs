using DataVault.Core.Api;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.ComponentModel
{
    public class ValueEditor : ElementEditor
    {
        protected override bool ApproveSelection(DataVaultUIContext context, IElement el)
        {
            return el is IValue;
        }
    }
}