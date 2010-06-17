using System;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Reflection;

namespace DataVault.UI.Api.ContentTypez
{
    public class ContentType : IContentType
    {
        private Type Type { get; set; }

        public ContentType(Type type)
        {
            Type = type.AssertNotNull();
            Type.HasAttr<ContentTypeAttribute>().AssertTrue();
            (Type.Attr<ContentTypeAttribute>().Token != "binary").AssertTrue();
            Type.HasAttr<ContentTypeLocAttribute>().AssertTrue();
            Type.GetConstructor(typeof(IBranch).MkArray());
            Type.GetConstructor(typeof(IValue).MkArray());
        }

        public string TypeToken { get { return Type.Attr<ContentTypeAttribute>().Token; } }

        private ContentTypeLocAttribute Loc { get { return Type.Attr<ContentTypeLocAttribute>(); } }
        public string LocTypeName { get { return Loc.TypeName; } }
        public string LocNewValue { get { return Loc.NewValue; } }
        public string LocValidationFailed { get { return Loc.ValidationFailed; } }

        public IContentTypeAppliedToValue Apply(IBranch parentOfNewValue)
        {
            var ctor = Type.GetConstructor(typeof(IBranch).MkArray());
            return (IContentTypeAppliedToValue)ctor.Invoke(parentOfNewValue.MkArray());
        }

        public IContentTypeAppliedToValue Apply(IValue existingValue)
        {
            var ctor = Type.GetConstructor(typeof(IValue).MkArray());
            return (IContentTypeAppliedToValue)ctor.Invoke(existingValue.MkArray());
        }
    }
}