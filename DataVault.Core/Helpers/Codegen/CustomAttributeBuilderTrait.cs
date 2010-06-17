using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace DataVault.Core.Helpers.Codegen
{
    public static class CustomAttributeBuilderTrait
    {
        public static CustomAttributeBuilder ToCustomAttributeBuilder(this CustomAttributeData data)
        {
            return new CustomAttributeBuilder
                (
                data.Constructor,
                data.ConstructorArguments.Select(x => x.Value).ToArray(),
                data.NamedArguments.Where(x => x.MemberInfo is PropertyInfo).Select(x => x.MemberInfo as PropertyInfo).ToArray(),
                data.NamedArguments.Where(x => x.MemberInfo is PropertyInfo).Select(x => x.TypedValue.Value).ToArray(),
                data.NamedArguments.Where(x => x.MemberInfo is FieldInfo).Select(x => x.MemberInfo as FieldInfo).ToArray(),
                data.NamedArguments.Where(x => x.MemberInfo is FieldInfo).Select(x => x.TypedValue.Value).ToArray()
                );
        }
    }
}