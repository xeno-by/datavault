using System.Reflection;

namespace DataVault.Core.Helpers.Reflection.Shortcuts
{
    public class TA
    {
        public const TypeAttributes Public = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoClass | TypeAttributes.AnsiClass;
        public const TypeAttributes PublicAbstract = TA.Public | TypeAttributes.Abstract;
        public const TypeAttributes PublicStatic = TA.Public | TypeAttributes.Abstract | TypeAttributes.Sealed;
    }
}