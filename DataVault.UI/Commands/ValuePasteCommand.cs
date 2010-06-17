using System;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;

namespace DataVault.UI.Commands
{
    public class ValuePasteCommand : ValueRelatedContextBoundCommand
    {
        private IValue CreatedValue { get; set; }

        public ValuePasteCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && ValueInClipboard != null && Branch != null;
        }

        public override void DoImpl()
        {
            Func<int, String> namegen = i => i == 1 ? ValueInClipboard.Name : String.Format(
                Resources.New_ValueDefaultPastedName, ValueInClipboard.Name, i - 1);
            var lastUsedIndex = 1.Seq(i => i + 1, i => Branch.GetValues().Any(b => b.Name == namegen(i))).LastOrDefault();
            var unusedName = namegen(lastUsedIndex + 1);

            CreatedValue = Branch
                .CreateValue(unusedName, () => ValueInClipboard.ContentStream)
                .SetDefault2()
                .SetTypeToken2(ValueInClipboard.GetTypeToken2());

            RefreshListAndThenSelect(CreatedValue);
        }

        public override void UndoImpl()
        {
            CreatedValue.Delete();
            RefreshListAndThenSelect(null);
        }
    }
}