using System;
using System.IO;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.ContentTypez;

namespace DataVault.UI.Commands
{
    public class ValueEditTypedFinishCommand : ValueEditFinishCommand
    {
        private IContentTypeAppliedToValue Wrapper { get; set; }

        public ValueEditTypedFinishCommand(DataVaultUIContext context, IContentTypeAppliedToValue wrapper)
            : base(context)
        {
            Wrapper = wrapper;
            wrapper.Untyped.AssertNotNull();
        }

        protected override String NewTypeToken
        {
            get { return Wrapper.CType.TypeToken; }
        }

        protected override Stream NewContent
        {
            get { return Wrapper.AsStoredStream; }
        }
    }
}