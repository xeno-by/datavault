using System;
using System.Collections.Generic;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Extensibility;

namespace DataVault.UI.Api.ContentTypez
{
    public static class ContentTypes
    {
        private static IContentType[] _contentTypes;
        public static IEnumerable<IContentType> All
        {
            get
            {
                if (_contentTypes == null)
                {
                    var contentTypes = Codebase.Assemblies.SelectMany(a => a.GetTypes())
                        .Where(t => t.IsDefined(typeof(ContentTypeAttribute), false))
                        .Select(t => (IContentType)new ContentType(t));

                    (contentTypes.Select(ctype => ctype.TypeToken).Distinct().Count() == contentTypes.Count()).AssertTrue();
                    contentTypes.Any(ctype => ctype.TypeToken == "binary").AssertFalse();

                    _contentTypes = contentTypes.ToArray();
                }

                return _contentTypes;
            }
        }

        public static IContentType GetCTypeFromToken(this String s)
        {
            var ctype = All.SingleOrDefault(ct => ct.TypeToken == s);
            return ctype ?? new UnknownContentType(s);
        }

        public static IContentTypeAppliedToValue ApplyCType(IBranch parentOfNewValue, String typeToken)
        {
            var ctype = typeToken.GetCTypeFromToken().AssertNotNull();
            return ctype.Apply(parentOfNewValue);
        }

        public static IContentTypeAppliedToValue ApplyCType(IValue existingValue)
        {
            var typeToken = existingValue.GetTypeToken2();
            var ctype = typeToken.GetCTypeFromToken().AssertNotNull();
            return ctype.Apply(existingValue);
        }
    }
}