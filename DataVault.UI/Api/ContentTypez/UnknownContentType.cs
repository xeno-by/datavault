using System;
using DataVault.Core.Api;
using DataVault.UI.Api.Exceptions;
using DataVault.UI.Properties;

namespace DataVault.UI.Api.ContentTypez
{
    public class UnknownContentType : IContentType
    {
        public String TypeToken { get; private set; }

        public UnknownContentType(string typeToken)
        {
            TypeToken = typeToken;
        }

        public string LocTypeName
        {
            get { return String.Format(Resources.ValueType_Unknown_Warning, TypeToken); }
        }

        public string LocNewValue
        {
            get { throw new ValidationException(Resources.ValueType_Unknown_Fatal, TypeToken); }
        }

        public string LocValidationFailed
        {
            // gets the same treatment as TextContentType
            get { return ((ContentTypeLocAttribute)null).ValidationFailed; }
        }

        public IContentTypeAppliedToValue Apply(IBranch parentOfNewValue)
        {
            return new UnknownContentTypeAppliedToValue(this, parentOfNewValue);
        }

        public IContentTypeAppliedToValue Apply(IValue existingValue)
        {
            return new UnknownContentTypeAppliedToValue(this, existingValue);
        }
    }
}