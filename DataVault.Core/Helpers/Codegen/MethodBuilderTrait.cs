using System.Reflection.Emit;

namespace DataVault.Core.Helpers.Codegen
{
    public static class MethodBuilderTrait
    {
        public static void ImplementByDefault(this MethodBuilder source)
        {
            source.il().lddefault(source.ReturnParameter.ParameterType).ret();
        }
    }
}