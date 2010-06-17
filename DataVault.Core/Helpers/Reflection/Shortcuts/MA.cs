using System.Reflection;

namespace DataVault.Core.Helpers.Reflection.Shortcuts
{
    public class MA
    {
        public const MethodAttributes Abstract = MethodAttributes.Abstract;

        public const MethodAttributes Private = MethodAttributes.Private | MethodAttributes.HideBySig;
        public const MethodAttributes PrivateProp = MethodAttributes.Private | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        public const MethodAttributes Protected = MethodAttributes.Family | MethodAttributes.HideBySig;
        public const MethodAttributes ProtectedProp = MethodAttributes.Family | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        public const MethodAttributes Public = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig;
        public const MethodAttributes PublicStatic = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
        public const MethodAttributes PublicCtor = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;
        public const MethodAttributes PublicProp = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
    }
}