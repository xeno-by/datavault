using System;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;

namespace DataVault.UI.Commands
{
    public abstract class ValueNewCommand : ValueRelatedContextBoundCommand
    {
        protected IValue CreatedValue { get; set; }

        protected ValueNewCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Branch != null;
        }

        public override void DoImpl()
        {
            CreatedValue = CreateValue();
            RefreshListAndThenSelect(CreatedValue).BeginEdit();
        }

        protected String CalculateFirstUnusedName()
        {
            Func<int, String> namegen = i => String.Format(Resources.New_ValueDefaultName, i);
            var lastUsedIndex = 1.Seq(i => i + 1, i => Branch.GetValues().Any(b => b.Name == namegen(i))).LastOrDefault();
            return namegen(lastUsedIndex + 1);
        }

        protected abstract IValue CreateValue();

        public override void UndoImpl()
        {
            CreatedValue.Delete();
            RefreshListAndThenSelect(null);
        }
    }
}