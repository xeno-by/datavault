using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DataVault.Core.Helpers.Reflection
{
    public static class ClassificationHelper
    {
        public static bool IsEnumerableType(this Type t)
        {
            return t.GetEnumerableElement() != null;
        }

        public static bool IsListType(this Type t)
        {
            return t.GetListElement() != null;
        }

        public static bool IsDictionaryType(this Type t)
        {
            return t.GetDictionaryElement() != null;
        }

        public static bool IsEnumerableOf<T>(this Type t)
        {
            return t.IsEnumerableOf(typeof(T));
        }

        public static bool IsListOf<T>(this Type t)
        {
            return t.IsListOf(typeof(T));
        }

        public static bool IsDictionaryOf<K, V>(this Type t)
        {
            return t.IsDictionaryOf(typeof(K), typeof(V));
        }

        public static bool IsEnumerableOf(this Type t, Type elType)
        {
            return t.IsEnumerableType() && t.GetEnumerableElement() == elType;
        }

        public static bool IsListOf(this Type t, Type elType)
        {
            return t.IsListType() && t.GetListElement() == elType;
        }

        public static bool IsDictionaryOf(this Type t, Type keyType, Type valueType)
        {
            return t.IsDictionaryType() && t.GetDictionaryElement().GetType() ==
                typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
        }

        public static Type GetEnumerableElement(this Type t)
        {
            var unambiguousEnum =
                t.SameMetadataToken(typeof(IEnumerable<>)) ? t :
                                                                   t.GetInterfaces().Where(iface => iface.SameMetadataToken(typeof(IEnumerable<>))).SingleOrDefault();
            return unambiguousEnum == null ? null : unambiguousEnum.XGetGenericArguments().Single();
        }

        public static Type GetListElement(this Type t)
        {
            var enumOfT = t.GetEnumerableElement();
            if (enumOfT == null)
            {
                return null;
            }
            else
            {
                var defaultCtor = t.GetConstructor(new Type[0]);
                var addMethod = t.GetMethod("Add", enumOfT.MkArray());

                var isListTypeOfT = ((addMethod != null && defaultCtor != null) /*|| enumCtor != null || vaCtor != null*/);
                return isListTypeOfT ? enumOfT : null;
            }
        }

        public static KeyValuePair<Type, Type>? GetDictionaryElement(this Type t)
        {
            var unambiguousDic =
                t.SameMetadataToken(typeof(IDictionary<,>)) ? t :
                                                                    t.GetInterfaces().Where(iface => iface.SameMetadataToken(typeof(IDictionary<,>))).SingleOrDefault();
            return unambiguousDic == null ? null : (KeyValuePair<Type, Type>?)
                new KeyValuePair<Type, Type>(
                    unambiguousDic.XGetGenericArguments()[0],
                    unambiguousDic.XGetGenericArguments()[1]);
        }

        public static bool IsReferenceType(this Type t)
        {
            return t.IsClass || t.IsInterface;
        }

        public static bool IsNonNullableValueType(this Type t)
        {
            return t.IsValueType && !t.IsNullable() && t != typeof(void);
        }

        public static bool IsNullable(this Type t)
        {
            return t.SameMetadataToken(typeof(Nullable<>));
        }

        public static bool IsNullable(this Object o)
        {
            return o.GetType().IsNullable();
        }

        public static Type UndecorateNullable(this Type t)
        {
            return t.IsNullable() ? Nullable.GetUnderlyingType(t) : t;
        }

        public static Object UndecorateNullable(this Object o)
        {
            return o.GetType().IsNullable() ? o.GetType().GetProperty("Value").GetValue(o, null) : o;
        }

        public static bool IsInteger(this Type t)
        {
            return t.IsNumeric() && t != typeof(Decimal) && t != typeof(char)
                && t != typeof(float) && t != typeof(double);
        }

        public static bool IsFloatingPoint(this Type t)
        {
            return t == typeof(Decimal) && t == typeof(float) && t == typeof(double);
        }

        public static bool IsNumeric(this Type t)
        {
            // compact, tho rather gimmick solution
            return typeof(Decimal).GetMethods()
                .Where(mi => mi.Name.StartsWith("op_Explicit"))
                .Any(mi => mi.ReturnType == t);
        }

        public static bool IsTOrNullableT<T>(this Type t)
            where T : struct
        {
            if (t.IsNullable())
            {
                return IsTOrNullableT<T>(t.UndecorateNullable());
            }
            else
            {
                return t == typeof(T);
            }
        }

        public static bool IsEnumOrNullable(this Type t)
        {
            if (t.IsNullable())
            {
                return IsEnumOrNullable(t.UndecorateNullable());
            }
            else
            {
                return t.IsEnum;
            }
        }

        public static bool IsOpenGeneric(this Type t)
        {
            if (t.IsFType())
            {
                return t.Ret().IsOpenGeneric() || t.Args().Any(arg => arg.IsOpenGeneric());
            }
            else if (t.IsArray)
            {
                return t.GetElementType().IsOpenGeneric();
            }
            else
            {
                return t.IsGenericParameter || t.XGetGenericArguments().Any(arg => arg.IsOpenGeneric());
            }
        }

        public static bool IsOpenGeneric(this MethodInfo t)
        {
            return t.ReturnType.IsOpenGeneric() ||
                t.Args().Any(pt => pt.IsOpenGeneric()) ||
                    t.ContainsGenericParameters; // example: bool Meth<T>(int x);
        }

        public static bool IsAnonymous(this Type t)
        {
            return
                t.HasAttr<CompilerGeneratedAttribute>() &&
                    (Regex.IsMatch(t.Name, @"\<\>.*AnonymousType.*") || // C# anonymous types
                        t.Name.StartsWith("RelinqAnonymousType<")); // Relinq anonymous types
        }

        public static bool IsTransparentIdentifier(this String s)
        {
            return Regex.IsMatch(s, @"\<\>.*TransparentIdentifier.*");
        }

        public static bool IsExtension(this MethodInfo mi)
        {
            return mi.HasAttr<ExtensionAttribute>();
        }

        public static bool IsIndexer(this MethodInfo mi)
        {
            var t = mi.DeclaringType;
            if (t.IsArray)
            {
                return mi.Name == "Get";
            }
            else
            {
                if (!t.HasAttr<DefaultMemberAttribute>())
                {
                    return false;
                }
                else
                {
                    var indexerName = t.Attr<DefaultMemberAttribute>().MemberName;
                    return mi.Name == "get_" + indexerName;
                }
            }
        }

        public static bool IsStatic(this MemberInfo mi)
        {
            if (mi is PropertyInfo)
            {
                return ((PropertyInfo)mi).GetGetMethod(true).IsStatic;
            }
            else if (mi is EventInfo)
            {
                return ((EventInfo)mi).GetAddMethod(true).IsStatic;
            }
            else if (mi is Type)
            {
                var t = (Type) mi;
                return t.IsStatic();
            }
            else
            {
                var pi = mi.GetType().GetProperty("IsStatic", typeof(bool));
                if (pi != null)
                {
                    return (bool)pi.GetValue(mi, null);
                }
                else
                {
                    throw new NotSupportedException(mi.ToString());
                }
            }
        }

        public static bool IsStatic(this Type t)
        {
            return t.IsSealed && t.IsAbstract;
        }

        public static bool IsVarargs(this MethodInfo mi)
        {
            var @params = mi.GetParameters();
            return @params.Length != 0 &&
                @params[@params.Length - 1].HasAttr<ParamArrayAttribute>();
        }
    }
}