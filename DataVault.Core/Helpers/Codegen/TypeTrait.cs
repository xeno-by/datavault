using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataVault.Core.Helpers.Codegen
{
    public static class TypeTrait
    {
        public static bool HasGenericTypeDefinition(this Type source, Type getericTypeDefinition)
        {
            if (!source.IsGenericType) return false;
            return source.GetGenericTypeDefinition() == getericTypeDefinition;
        }

        public static bool HasMethod(this Type source, out MethodInfo method, MethodAttributes attributes, Type returnType, String name, params Type[] parameterTypes)
        {
            method = null;

            var methods = 
                from m in source.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                where 
                    m.Name == name && 
                        (m.Attributes & attributes) == attributes && 
                            m.ReturnType == returnType && 
                                parameterTypes.AreEqual(m.GetParameters())
                select m;

            if (methods.Count() == 1) method = methods.First();
            return method != null;
        }

        public static bool AreEqual(this Type[] source, params ParameterInfo[] destination)
        {
            if (source.Length != destination.Length) return false;
            for (var i = 0; i < source.Length; i++)
            {
                if (source[i] != destination[i].ParameterType) return false;
            }

            return true;
        }

        public static IEnumerable<Type> GetAllInterfaces(this Type source)
        {
            return source.IsInterface ? source.GetInterfaces().Union(new[] {source}) : source.GetInterfaces();
        }

        public static String ToShortString(this Type source)
        {
            var buff = new StringBuilder(256);

            if (source.IsGenericType)
            {
                buff.Append(source.Name.Substring(0, source.Name.IndexOf('`')))
                    .Append('<')
                    .Append(String.Join(",", source.GetGenericArguments().Select(x => x.ToShortString()).ToArray()))
                    .Append('>');
            }
            else if (source.HasElementType)
            {
                buff.Append(source.GetElementType().ToShortString())
                    .Append("[]");
            }
            else
            {
                buff.Append(source.Name);
            }

            return buff.ToString();
        }
    }
}