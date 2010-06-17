using System;
using System.ComponentModel;
using System.Globalization;
using DataVault.Core.Api;

namespace DataVault.UI.Api.ComponentModel
{
    public class ElementTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(String);
        }

        public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object value, Type destinationType)
        {
            return value == null ? null : ((IElement)value).VPath.ToString();
        }
    }
}