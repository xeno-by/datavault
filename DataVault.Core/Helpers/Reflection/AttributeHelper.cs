using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataVault.Core.Helpers.Reflection.Shortcuts;

namespace DataVault.Core.Helpers.Reflection
{
    public static class AttributeHelper
    {
        public static bool HasAttr<T>(this ICustomAttributeProvider cap)
            where T : Attribute
        {
            return cap.HasAttr<T>(true);
        }

        public static bool HasAttr<T>(this ICustomAttributeProvider cap, bool inherit)
            where T : Attribute
        {
            // a crude workaround
            if (cap is PropertyInfo && inherit)
            {
                var pi = (PropertyInfo)cap;
                var hierarchy = pi.DeclaringType.LookupInheritanceChain();
                return hierarchy.Any(t =>
                {
                    var basep = t.GetProperties(BF.All).SingleOrDefault(p => p.Name == pi.Name && p.DeclaringType == t);
                    return basep == null ? false : basep.IsDefined(typeof(T), true);
                });
            }
            else
            {
                return cap.IsDefined(typeof(T), inherit);
            }
        }

        public static IEnumerable<T> Attrs<T>(this ICustomAttributeProvider cap)
            where T : Attribute
        {
            return cap.Attrs<T>(true);
        }

        public static IEnumerable<T> Attrs<T>(this ICustomAttributeProvider cap, bool inherit)
            where T : Attribute
        {
            // a crude workaround
            if (cap is PropertyInfo && inherit)
            {
                var pi = (PropertyInfo)cap;
                var hierarchy = pi.DeclaringType.LookupInheritanceChain();
                return hierarchy.SelectMany(t =>
                {
                    var basep = t.GetProperties(BF.All).SingleOrDefault(p => p.Name == pi.Name && p.DeclaringType == t);
                    return basep == null ? Enumerable.Empty<T>() : basep.GetCustomAttributes(typeof(T), true).Cast<T>();
                })
                // this is necessary to shield us from a codegen bug
                // typeof(Gen'dNode).GetProperties("Name", any BF) returns 2x props: CompiledNode.Name and Gen'dNode.Name
                .TakeWhile((a, i) => i < 1);
            }
            else
            {
                return cap.GetCustomAttributes(typeof(T), inherit).Cast<T>();
            }
        }

        public static T Attr<T>(this ICustomAttributeProvider cap)
            where T : Attribute
        {
            return cap.Attr<T>(true);
        }

        public static T Attr<T>(this ICustomAttributeProvider cap, bool inherit)
            where T : Attribute
        {
            return cap.Attrs<T>().Single();
        }

        public static T AttrOrNull<T>(this ICustomAttributeProvider cap)
            where T : Attribute
        {
            return cap.AttrOrNull<T>(true);
        }

        public static T AttrOrNull<T>(this ICustomAttributeProvider cap, bool inherit)
            where T : Attribute
        {
            return cap.Attrs<T>().SingleOrDefault();
        }
    }
}