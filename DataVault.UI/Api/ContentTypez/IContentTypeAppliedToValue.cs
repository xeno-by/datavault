using System;
using System.IO;
using DataVault.Core.Api;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.ContentTypez
{
    public interface IContentTypeAppliedToValue
    {
        IValue Untyped { get; }
        IContentType CType { get; }

        Object Value { get; }
        String TypeToken { get; }

        String AsLocalizedString { get; set; }
        String AsStoredString { get; set; }
        Stream AsStoredStream { get; set; }

        ICommand IssueApplyChangesCommand(DataVaultUIContext context);
    }

    public interface IContentTypeAppliedToValue<T> : IContentTypeAppliedToValue
    {
        new T Value { get; }
    }
}
