using System;
using DataVault.Core.Api;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.ContentTypez;

namespace DataVault.UI.Commands
{
    public class ValueNewTypedCommand : ValueNewCommand
    {
        private IContentTypeAppliedToValue Wrapper { get; set; }

        public ValueNewTypedCommand(DataVaultUIContext context, String typeToken)
            : base(context)
        {
            Wrapper = ContentTypes.ApplyCType(Branch, typeToken);
        }

        public ValueNewTypedCommand(DataVaultUIContext context, IContentTypeAppliedToValue wrapper)
            : base(context)
        {
            Wrapper = wrapper;
        }

        protected override IValue CreateValue()
        {
            var createdValue = Branch.CreateValue(CalculateFirstUnusedName(), () => Wrapper.AsStoredStream).SetDefault2();
            createdValue.SetTypeToken2(Wrapper.TypeToken);
            return createdValue;
        }
    }
}
