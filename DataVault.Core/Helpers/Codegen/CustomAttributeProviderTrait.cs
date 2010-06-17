using System;
using System.Reflection;

namespace DataVault.Core.Helpers.Codegen
{
    public static class CustomAttributeProviderTrait
    {
        public static bool IsAnnotated<A>(this ICustomAttributeProvider source) where A : Attribute
        {
            return IsAnnotated<A>(source, null);
        }

        public static bool IsAnnotated<A>(this ICustomAttributeProvider source, Predicate<A> predicate) where A : Attribute
        {
            if (source == null) throw new ArgumentNullException("source");

            var cas = source.GetCustomAttributes(typeof(A), true);
            if (cas.Length == 0) return false;
            if (predicate == null) return true;
            foreach (var ca in cas) if (predicate((A)ca)) return true;

            return false;
        }

        public static bool HasAnnotation<A>(this ICustomAttributeProvider source, out A annotation) where A : Attribute
        {
            if (source == null) throw new ArgumentNullException("source");

            var cas = source.GetCustomAttributes(typeof(A), true);
            if (cas.Length > 0)
            {
                annotation = (A)cas[0];
                return true;
            }

            annotation = null;
            return false;
        }
    }
}