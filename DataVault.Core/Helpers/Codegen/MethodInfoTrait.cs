using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataVault.Core.Helpers.Codegen
{
    public static class MethodInfoTrait
    {
        public static bool IsInvariantTo(this MethodInfo source, MethodInfo method)
        {
            if (source.IsGenericMethod || method.IsGenericMethod) throw new NotImplementedException(); // TODO: implement it somewhen

            var me = source.GetParameters();
            var he = method.GetParameters();

            if (source.ReturnType != method.ReturnType && !source.ReturnType.IsAssignableFrom(method.ReturnType)) return false;
            if (me.Length != he.Length) return false;
            for (var i = 0; i < me.Length; i++)
                if (he[i].ParameterType != me[i].ParameterType && !he[i].ParameterType.IsAssignableFrom(me[i].ParameterType)) return false;

            return true;
        }

        public static PropertyInfo EnclosingProperty(this MethodInfo source)
        {
            if (source == null) return null;
            if (source.DeclaringType == null) return null;
            return (
                from prop in source.DeclaringType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                where prop.GetAccessors(true).Contains(source)
                select prop
                ).FirstOrDefault();
        }

        public static IEnumerable<MethodInfo> Declarations(this MethodInfo source)
        {
            if (source == null) return new MethodInfo[0];
            if (source.DeclaringType == null) return new MethodInfo[0];
            if (source.DeclaringType.IsInterface) return new MethodInfo[0];

            return from iface in source.DeclaringType.GetInterfaces()
            from entry in source.DeclaringType.GetInterfaceMap(iface).ToDictionary()
            where entry.Key == source
            select entry.Value;
        }

        public static String ToShortString(this MethodInfo source)
        {
            var buff = new StringBuilder(256);
            buff.Append(source.DeclaringType != null ? source.DeclaringType.ToShortString() : "none")
                .Append("::");

            if (source.IsGenericMethod)
            {
                buff.Append(source.Name.Substring(0, source.Name.IndexOf('`')))
                    .Append('<')
                    .Append(String.Join(",", source.GetGenericArguments().Select(x => x.ToShortString()).ToArray()))
                    .Append('>');
            }
            else
            {
                buff.Append(source.Name);
            }

            buff.Append('(')
                .Append(String.Join(", ", source.GetParameters().Select(x => x.ParameterType.ToShortString()).ToArray()))
                .Append("): ")
                .Append(source.ReturnType.ToShortString());

            return buff.ToString();
        }
    }
}