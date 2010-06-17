using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Exceptions;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.ContentTypez
{
    public abstract class CultureAwareContentTypeAppliedToValue<T> : ContentTypeAppliedToValue<T>
    {
        protected CultureAwareContentTypeAppliedToValue(IBranch parent)
            : base(parent)
        {
        }

        protected CultureAwareContentTypeAppliedToValue(IValue untyped) 
            : base(untyped) 
        {
        }

        protected sealed override String ConvertToInvariantString(T t)
        {
            return ConvertToString(t, CultureInfo.InvariantCulture);
        }

        protected sealed override String ConvertToLocalizedString(T t)
        {
            return ConvertToString(t, Thread.CurrentThread.CurrentUICulture);
        }

        protected sealed override T ConvertFromInvariantString(String s)
        {
            return ConvertFromString(s, CultureInfo.InvariantCulture);
        }

        protected sealed override T ConvertFromLocalizedString(String s)
        {
            return ConvertFromString(s, Thread.CurrentThread.CurrentUICulture);
        }

        protected virtual String ConvertToString(T t, CultureInfo culture)
        {
            var tc = TypeDescriptor.GetConverter(typeof(T)).AssertNotNull();
            tc.CanConvertTo(typeof(String)).AssertTrue();
            return (String)tc.ConvertTo(null, culture, t, typeof(String));
        }

        protected virtual T ConvertFromString(String s, CultureInfo culture)
        {
            var tc = TypeDescriptor.GetConverter(typeof(T)).AssertNotNull();
            tc.CanConvertFrom(typeof(String)).AssertTrue();

            try
            {
                return (T)tc.ConvertFrom(null, culture, s);
            }
            catch (Exception e)
            {
                throw new ValidationException(CType.LocValidationFailed, e, s);
            }
        }
    }
}