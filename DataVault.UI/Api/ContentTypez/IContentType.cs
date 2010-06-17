using System;
using DataVault.Core.Api;

namespace DataVault.UI.Api.ContentTypez
{
    public interface IContentType
    {
        String TypeToken { get; }

        String LocTypeName { get;}
        String LocNewValue { get; }
        String LocValidationFailed { get; }

        IContentTypeAppliedToValue Apply(IBranch parentOfNewValue);
        IContentTypeAppliedToValue Apply(IValue existingValue);
    }
}