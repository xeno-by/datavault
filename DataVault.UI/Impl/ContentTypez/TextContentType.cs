using System;
using DataVault.Core.Api;
using DataVault.UI.Api.ContentTypez;
using DataVault.UI.Properties;

namespace DataVault.UI.Impl.ContentTypez
{
    [ContentType("text"), ContentTypeLoc(typeof(Resources), "ValueType_Text", "New_ValueText", null)]
    internal class TextContentType : CultureAwareContentTypeAppliedToValue<String>
    {
        public TextContentType(IBranch parent)
            : base(parent)
        {
        }

        public TextContentType(IValue untyped)
            : base(untyped)
        {
        }
    }
}