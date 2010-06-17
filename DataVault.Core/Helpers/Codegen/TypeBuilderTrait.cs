using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DataVault.Core.Helpers.Reflection;

namespace DataVault.Core.Helpers.Codegen
{
    public static class TypeBuilderTrait
    {
        public static MethodBuilder OverrideMethod(this TypeBuilder source, MethodInfo parentMethod)
        {
            return OverrideMethod(source, parentMethod, null);
        }

        public static MethodBuilder OverrideMethod(this TypeBuilder source, MethodInfo parentMethod, Func<ILGenerator, ILGenerator> body)
        {
            return OverrideMethod(source, parentMethod, body, null);
        }

        public static MethodBuilder OverrideMethod(this TypeBuilder source, MethodInfo parentMethod, Func<ILGenerator, ILGenerator> body, IDictionary<MethodInfo, MethodBuilder> map)
        {
            var derived = source.DefineMethod(
                // that's an awesome idea but it hurts reflector and debuggability
                //                String.Format("{0}_{1}", parentMethod.Name, parentMethod.DeclaringType.ToShortString()),
                parentMethod.Name,
                MethodAttributes.Final | MethodAttributes.Public | MethodAttributes.Virtual,
                parentMethod.ReturnType,
                parentMethod.Args());

            if (body != null) body(derived.il());

            source.DefineMethodOverride(derived, parentMethod);
            if (map != null) map[parentMethod] = derived;
            return derived;
        }
    }
}