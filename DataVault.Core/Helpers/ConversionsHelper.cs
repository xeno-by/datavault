using System;
using System.ComponentModel;
using System.Globalization;

namespace DataVault.Core.Helpers
{
    public static class ConversionsHelper
    {
        public static bool SupportsSerializationToString<T>()
        {
            return SupportsSerializationToString(typeof(T));
        }

        public static bool SupportsSerializationToString(this Type t)
        {
            var converter = TypeDescriptor.GetConverter(t);
            return converter.CanConvertTo(typeof(String)) && converter.CanConvertFrom(typeof(String));
        }

        public static T FromInvariantString<T>(this String s)
        {
            return (T)FromInvariantString(typeof(T), s);
        }

        public static Object FromInvariantString(this Type t, String s)
        {
            if (s == null)
            {
                return null;
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(t);
                if (converter.CanConvertFrom(typeof(String)))
                {
                    return converter.ConvertFromInvariantString(s);
                }
                else
                {
                    throw new NotSupportedException(t.ToString());
                }
            }
        }

        public static String ToInvariantString(this Object @object)
        {
            if (@object == null)
            {
                return null;
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(@object);
                if (converter.CanConvertTo(typeof(String)))
                {
                    return converter.ConvertToInvariantString(@object);
                }
                else
                {
                    return @object.ToString();
                }
            }
        }

        public static T FromLocalString<T>(this String s, CultureInfo locale)
        {
            return (T)FromLocalString(typeof(T), s, locale);
        }

        public static Object FromLocalString(this Type t, String s, CultureInfo locale)
        {
            if (s == null)
            {
                return null;
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(t);
                if (converter.CanConvertFrom(typeof(String)))
                {
                    return converter.ConvertFromString(null, locale, s);
                }
                else
                {
                    throw new NotSupportedException(t.ToString());
                }
            }
        }

        public static String ToLocalString(this Object @object, CultureInfo locale)
        {
            if (@object == null)
            {
                return null;
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(@object);
                if (converter.CanConvertTo(typeof(String)))
                {
                    return converter.ConvertToString(null, locale, @object);
                }
                else
                {
                    return @object.ToString();
                }
            }
        }
    }
}