using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataVault.Core.Helpers.Reflection.Shortcuts;

namespace DataVault.Core.Helpers.Reflection
{
    public static class ReflectionHelper
    {
        public static Type GetFieldOrPropertyType(this Type t, String name)
        {
            return t.GetFieldOrProperty(name).GetFieldOrPropertyType();
        }

        public static Type GetFieldOrPropertyType(this MemberInfo mi)
        {
            if (mi == null)
            {
                return null;
            }
            else
            {
                return mi is FieldInfo ? ((FieldInfo)mi).FieldType : ((PropertyInfo)mi).PropertyType;
            }
        }

        public static MemberInfo GetFieldOrProperty(this Type t, String name)
        {
            if (t == null)
            {
                return null;
            }

            // only public for now
            var fi = t.GetField(name, BF.PublicInstance);
            if (fi != null)
            {
                return fi;
            }

            var pi = t.GetProperty(name, BF.PublicInstance);
            if (pi != null)
            {
                return pi;
            }

            return null;
        }

        // Has a multitude of uses, but primary of those is  finding out 
        // whether some type is a closed generic for some other type
        // e.g. SameMetadataToken(List<int>, List<>) will return true
        // for comparison, typeof(List<int>) == typeof(List<>) will return false
        public static bool SameMetadataToken(this MemberInfo t1, MemberInfo t2)
        {
            return t1.Module.Assembly == t2.Module.Assembly &&
                t1.Module == t2.Module &&
                    t1.MetadataToken == t2.MetadataToken;
        }

        public static bool SameType(this Type t1, Type t2)
        {
            if (!t1.SameBasisType(t2))
            {
                return false;
            }
            else
            {
                var t1args = t1.XGetGenericArguments();
                var t2args = t2.XGetGenericArguments();

                if (t1args.Length != t2args.Length)
                {
                    throw new NotSupportedException(String.Format("Something was overlooked: " +
                        "The type '{0}' and '{1}' claimed that they share basis type", t1, t2));
                }
                else
                {
                    return t1args.AllMatch(t2args,
                        (t1argsi, t2argsi) => t1argsi.SameType(t2argsi));
                }
            }
        }

        public static Type GetBasisType(this Type t)
        {
            return t.XGetGenericDefinition();
        }

        public static bool SameBasisType(this Type t1, Type t2)
        {
            if (t1 == null || t2 == null)
            {
                return t1 == t2;
            }
            else
            {
                return t1.SameMetadataToken(t2);
            }
        }

        public static Type[] GetBaseTypes(this Type t)
        {
            if (t.IsInterface)
            {
                return t.GetInterfaces().Concat(typeof(object)).ToArray();
            }
            else
            {
                var baseTypes = new List<Type>();
                for (var current = t.BaseType; current != null; current = current.BaseType)
                    baseTypes.Add(current);
                return baseTypes.ToArray();
            }
        }

        public static IEnumerable<Type> LookupInheritanceChain(this Type t)
        {
            if (t == null)
            {
                yield break;
            }
            else
            {
                yield return t;

                for (var current = t.BaseType; current != null; current = current.BaseType)
                    yield return current;

                foreach (var baseIface in t.GetInterfaces())
                    yield return baseIface;
            }
        }
    }
}