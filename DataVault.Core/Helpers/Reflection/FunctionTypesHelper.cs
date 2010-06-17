using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Helpers.Reflection
{
    public static class FunctionTypesHelper
    {
        public static bool IsDelegate(this Type t)
        {
            if (t.SameMetadataToken(typeof(Delegate))) return false;
            if (t.SameMetadataToken(typeof(MulticastDelegate))) return false;

            for (var current = t; current != null; current = current.BaseType)
                if (current.SameMetadataToken(typeof(Delegate))) return true;

            return false;
        }

        public static bool IsFType(this Type t)
        {
            return t.IsDelegate();
        }

        public static bool IsAction(this Type t)
        {
            return t.IsFType() && t.Ret() == typeof(void);
        }

        public static bool IsFunc(this Type t)
        {
            return t.IsFType() && t.Ret() != typeof(void);
        }

        public static Type Ret(this MethodInfo mi)
        {
            return mi.ReturnType;
        }

        public static Type Arg(this MethodInfo mi, int i)
        {
            return mi.GetParameters()[i].ParameterType;
        }

        public static int Argc(this MethodInfo mi)
        {
            return mi.Args().Count();
        }

        public static Type[] Args(this MethodInfo mi)
        {
            return mi.GetParameters().Select(p => p.ParameterType).ToArray();
        }

        public static Type Ret(this Type t)
        {
            t.IsFType().AssertTrue();
            return t.GetFunctionSignature().Ret();
        }

        public static Type Arg(this Type t, int i)
        {
            t.IsFType().AssertTrue();
            return t.GetFunctionSignature().Arg(i);
        }

        public static int Argc(this Type t)
        {
            t.IsFType().AssertTrue();
            return t.GetFunctionSignature().Argc();
        }

        public static Type[] Args(this Type t)
        {
            t.IsFType().AssertTrue();
            return t.GetFunctionSignature().Args();
        }

        public static MethodInfo GetFunctionSignature(this Type t)
        {
            t.IsFType().AssertTrue();
            if (t.IsDelegate())
            {
                return t.GetMethod("Invoke");
            }
            else
            {
                throw AssertionHelper.Fail();
            }
        }

        public static Type GetSignatureFunction(this MethodInfo t)
        {
            throw new NotImplementedException();
        }

        public static Type ToFunc(this int argCount)
        {
            switch (argCount)
            {
                case 0:
                    return typeof(Func<>);
                case 1:
                    return typeof(Func<,>);
                case 2:
                    return typeof(Func<,,>);
                case 3:
                    return typeof(Func<,,,>);
                case 4:
                    return typeof(Func<,,,,>);
                case 5:
                    return typeof(Func<,,,,,>);
                case 6:
                    return typeof(Func<,,,,,,>);
                case 7:
                    return typeof(Func<,,,,,,,>);
                case 8:
                    return typeof(Func<,,,,,,,,>);
                default:
                    throw new NotSupportedException(String.Format(
                        "Funcs with '{0}' arg(s) are not supported", argCount));
            }
        }

        public static Type ToFunc(this IEnumerable<Type> args)
        {
            var genericDef = ToFunc(args.Count() - 1);
            return genericDef.XMakeGenericType(args.ToArray());
        }

        public static Type ToFunc(this IEnumerable<Type> args, Type retVal)
        {
            return args.Concat(retVal).ToFunc();
        }

        public static Type ToAction(this int argCount)
        {
            switch (argCount)
            {
                case 0:
                    return typeof(Action);
                case 1:
                    return typeof(Action<>);
                case 2:
                    return typeof(Action<,>);
                case 3:
                    return typeof(Action<,,>);
                case 4:
                    return typeof(Action<,,,>);
                case 5:
                    return typeof(Action<,,,,>);
                case 6:
                    return typeof(Action<,,,,,>);
                case 7:
                    return typeof(Action<,,,,,,>);
                case 8:
                    return typeof(Action<,,,,,,,>);
                default:
                    throw new NotSupportedException(String.Format(
                        "Actions with '{0}' arg(s) are not supported", argCount));
            }
        }

        public static Type ToAction(this IEnumerable<Type> args)
        {
            var genericDef = ToAction(args.Count());
            return genericDef.XMakeGenericType(args.ToArray());
        }

        public static Type ForgeFType(this IEnumerable<Type> args)
        {
            if (args.LastOrDefault() == typeof(void))
            {
                return args.ToAction();
            }
            else
            {
                return args.Skip(1).ToFunc(args.Last());
            }
        }

        public static Type ToAction(this Type ftype)
        {
            return ftype.ToAction(t => true);
        }

        public static Type ToAction(this Type ftype, Func<Type, bool> argFilter)
        {
            ftype.IsFType().AssertTrue();
            return ftype.Args().Where(argFilter).ToAction();
        }

        public static Type ToAction(this Type ftype, Func<IEnumerable<Type>, IEnumerable<Type>> xform)
        {
            ftype.IsFType().AssertTrue();
            return xform(ftype.Args()).ToAction();
        }

        public static Type ToFunc(this Type ftype, Type ret)
        {
            return ftype.ToFunc(t => true, ret);
        }

        public static Type ToFunc(this Type ftype, Func<Type, bool> argFilter, Type ret)
        {
            ftype.IsFType().AssertTrue();
            return ftype.Args().Where(argFilter).ToFunc(ret);
        }

        public static Type ToFunc(this Type ftype, Func<IEnumerable<Type>, IEnumerable<Type>> xform, Type ret)
        {
            ftype.IsFType().AssertTrue();
            return xform(ftype.Args()).ToFunc(ret);
        }
    }
}