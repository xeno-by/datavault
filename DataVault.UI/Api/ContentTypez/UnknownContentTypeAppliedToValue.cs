using System;
using System.IO;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Commands;

namespace DataVault.UI.Api.ContentTypez
{
    public class UnknownContentTypeAppliedToValue : IContentTypeAppliedToValue
    {
        private IBranch Parent { get; set; }
        public IValue Untyped { get; private set; }
        private String Content { get; set; }

        IContentType IContentTypeAppliedToValue.CType { get { return CType; } }
        public IContentType CType { get; private set; }
        public String TypeToken { get { return CType.TypeToken; } }

        public UnknownContentTypeAppliedToValue(UnknownContentType ctype, IBranch parent)
        {
            Parent = parent;
            CType = ctype;
            Content = null;
        }

        public UnknownContentTypeAppliedToValue(UnknownContentType ctype, IValue untyped)
        {
            Untyped = untyped.AssertNotNull();
            CType = ctype;
            Content = untyped.ContentString;
        }

        public object Value
        {
            get { return AsStoredString; }
        }

        public string AsLocalizedString
        {
            get { return Content; }
            set { Content = value; }
        }

        public string AsStoredString
        {
            get { return Content; }
            set { Content = value; }
        }

        public Stream AsStoredStream
        {
            get { return AsStoredString.AsStream(); }
            set { AsStoredString = value.AsString(); }
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
    }
}