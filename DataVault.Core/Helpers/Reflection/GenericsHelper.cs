using System;
using System.Linq;
using System.Reflection;
using DataVault.Core.Helpers.Reflection.Shortcuts;

namespace DataVault.Core.Helpers.Reflection
{
    public static class GenericsHelper
    {
        public static Type[] XGetGenericArguments(this Type t)
        {
            if (t == null) return null;
            return t.IsGenericType ? t.GetGenericArguments() : new Type[0];
        }

        public static Type[] XGetGenericArguments(this MethodInfo mi)
        {
            if (mi == null) return null;
            return mi.IsGenericMethod ? mi.GetGenericArguments() : new Type[0];
        }

        public static Type XGetGenericDefinition(this Type t)
        {
            if (t == null) return null;
            return t.IsGenericType ? t.GetGenericTypeDefinition() : t;
        }

        public static MethodInfo XGetGenericDefinition(this MethodInfo mi)
        {
            if (mi == null) return null;
            return mi.IsGenericMethod ? mi.GetGenericMethodDefinition() : mi;
        }

        public static Type XMakeGenericType(this Type t, params Type[] targs)
        {
            if (t == null) return null;
            if (!t.IsGenericType || t.IsGenericParameter)
            {
                if (targs.Length != 0)
                {
                    throw new NotSupportedException(targs.Length.ToString());
                }
                else
                {
                    return t;
                }
            }
            else
            {
                return t.GetGenericTypeDefinition().MakeGenericType(targs);
            }
        }

        public static MethodInfo XMakeGenericMethod(this MethodInfo mi, params Type[] margs)
        {
            return mi.XMakeGenericMethod(mi.DeclaringType.XGetGenericArguments(), margs);
        }

        public static MethodInfo XMakeGenericMethod(this MethodInfo mi, Type[] targs, Type[] margs)
        {
            if (mi == null) return null;
            var pattern = (MethodInfo)mi.Module.ResolveMethod(mi.MetadataToken);

            var typeImpl = pattern.DeclaringType;
            if (!targs.IsNullOrEmpty()) typeImpl = typeImpl.MakeGenericType(targs);

            var methodImpl = typeImpl.GetMethods(BF.All).Single(mi2 => mi2.SameMetadataToken(mi));
            if (!margs.IsNullOrEmpty()) methodImpl = methodImpl.MakeGenericMethod(margs);

            return methodImpl;
        }
    }
}