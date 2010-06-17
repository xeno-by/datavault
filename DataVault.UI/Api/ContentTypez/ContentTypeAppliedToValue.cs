using System;
using System.IO;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Reflection;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Commands;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.ContentTypez
{
    public abstract class ContentTypeAppliedToValue<T> : IContentTypeAppliedToValue<T>
    {
        private IBranch Parent { get; set; }
        public IValue Untyped { get; private set; }

        public IContentType CType { get; private set; }
        public String TypeToken { get { return CType.TypeToken; } }

        public T Value { get { return ConvertFromInvariantString(AsStoredString); } }
        Object IContentTypeAppliedToValue.Value { get { return Value; } }

        public String AsLocalizedString 
        {
            get
            {
                return ConvertToLocalizedString(Value);
            }
            set
            {
                var typed = ConvertFromLocalizedString(value);
                AsStoredString = ConvertToInvariantString(typed);
            }
        }

        private String _updatedAsString;
        public String AsStoredString 
        {
            get { return _updatedAsString ?? (Untyped == null ? ConvertToInvariantString(default(T)) : Untyped.ContentString); }
            set
            {
                var typed = ConvertFromInvariantString(value);
                _updatedAsString = ConvertToInvariantString(typed);
            }
        }

        public Stream AsStoredStream
        {
            get { return AsStoredString.AsStream(); }
            set { AsStoredString = value.AsString(); }
        }

        protected ContentTypeAppliedToValue(IBranch parent)
        {
            // can be null if SelectedBranch is null
            Parent = parent;

            var ctypeAttr = this.GetType().Attr<ContentTypeAttribute>();
            CType = ctypeAttr.Token.GetCTypeFromToken();
        }

        protected ContentTypeAppliedToValue(IValue untyped)
        {
            Untyped = untyped.AssertNotNull();

            var ctypeAttr = this.GetType().Attr<ContentTypeAttribute>();
            CType = ctypeAttr.Token.GetCTypeFromToken();
        }

        public ICommand IssueApplyChangesCommand(DataVaultUIContext context)
        {
            if (Untyped == null)
            {
                return new ValueNewTypedCommand(context, this);
            }
            else
            {
                if (Untyped.ContentString != AsStoredString || 
                    Untyped.GetTypeToken2() != TypeToken)
                {
                    return new ValueEditTypedFinishCommand(context, this);
                }
                else
                {
                    return null;
                }
            }
        }

        protected abstract String ConvertToInvariantString(T t);
        protected abstract String ConvertToLocalizedString(T t);
        protected abstract T ConvertFromInvariantString(String s);
        protected abstract T ConvertFromLocalizedString(String s);
    }
}