using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Reflection;

namespace DataVault.Core.Helpers
{
    public static class ToStringHelper
    {
        public static String VerySafeToString(this Object o)
        {
            try
            {
                return o == null ? null : o.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static String GetCSharpTypeName(this Type t)
        {
            var unqualified = t.GetUnqualifiedCSharpTypeName();
            if (t.IsGenericParameter)
            {
                return unqualified;
            }
            else
            {
                var @namespace = "global::" +
                    (t.Namespace.IsNullOrEmpty() ? String.Empty : (t.Namespace + "."));
                return @namespace + unqualified;
            }
        }

        public static String GetUnqualifiedCSharpTypeName(this Type t)
        {
            var buffer = new StringBuilder();

            var name = t.Name;
            var apos = name.IndexOf("`");
            buffer.Append(apos == -1 ? name : name.Substring(0, apos));

            if (t.XGetGenericArguments().Count() > 0)
            {
                buffer.Append("<");
                buffer.Append(t.XGetGenericArguments().Select(targ => GetCSharpTypeName(targ)).StringJoin());
                buffer.Append(">");
            }

            return buffer.ToString();
        }

        public static String GetCSharpTypeArgsClause(this Type[] tt)
        {
            tt.ForEach(t => t.IsGenericParameter.AssertTrue());
            return tt.Count() == 0 ? String.Empty : "<" + tt.Select(t => t.GetCSharpTypeName()).StringJoin() + ">";
        }

        public static String GetCSharpTypeArgsClause(this Type t)
        {
            var ogargs = t.Flatten(t1 => t1.XGetGenericArguments()).Where(garg => garg.IsGenericParameter);
            return GetCSharpTypeArgsClause(ogargs.ToArray());
        }

        public static String GetCSharpTypeArgsClause(this MethodInfo mi)
        {
            Func<Type, IEnumerable<Type>> ogargs = t =>
                t.Flatten(t1 => t1.XGetGenericArguments()).Where(garg => garg.IsGenericParameter);
            var involvedTypes = mi.Args().Concat(mi.ReturnType).Concat(mi.DeclaringType);
            var m_ogargs = involvedTypes.SelectMany(ogargs).Distinct().ToArray();
            return GetCSharpTypeArgsClause(m_ogargs);
        }

        public static String GetCSharpOwnTypeArgsClause(this MethodInfo mi)
        {
            var ogargs = mi.XGetGenericArguments();
            return GetCSharpTypeArgsClause(ogargs.ToArray());
        }

        public static String GetCSharpTypeConstraintsClause(this Type t)
        {
            const string indent = "    ";
            var buffer = new StringBuilder();
            Action<String> appendConstraint = constraint => buffer.AppendLine(String.Format(
                "{0}where {1} : {2}", indent, t.GetCSharpTypeName(), constraint));

            var inheritance = t.GetGenericParameterConstraints();
            inheritance.ForEach(t_c => appendConstraint(t_c.GetCSharpTypeName()));

            if ((t.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) == GenericParameterAttributes.DefaultConstructorConstraint)
                appendConstraint("new()");
            if ((t.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) == GenericParameterAttributes.DefaultConstructorConstraint)
                appendConstraint("class");
            if ((t.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == GenericParameterAttributes.DefaultConstructorConstraint)
                appendConstraint("struct");

            return buffer.ToString();
        }

        public static String GetCSharpTypeConstraintsClause(this MethodInfo mi)
        {
            Func<Type, IEnumerable<Type>> ogargs = t =>
                t.Flatten(t1 => t1.XGetGenericArguments()).Where(garg => garg.IsGenericParameter);
            var m_ogargs = mi.Args().Concat(mi.ReturnType).SelectMany(ogargs).Distinct().ToArray();
            return m_ogargs.Select(ogarg => ogarg.GetCSharpTypeConstraintsClause()).StringJoin(Environment.NewLine);
        }
    }
}
