using System.Collections.Generic;
using System.Reflection;

namespace DataVault.Core.Helpers.Codegen
{
    public static class InterfaceMappingTrait
    {
        public static IDictionary<MethodInfo, MethodInfo> ToDictionary(this InterfaceMapping source)
        {
            var res = new Dictionary<MethodInfo, MethodInfo>();
            for (var i = 0; i < source.TargetMethods.Length; i++)
            {
                res.Add(source.TargetMethods[i], source.InterfaceMethods[i]);
            }

            return res;
        }
    }
}