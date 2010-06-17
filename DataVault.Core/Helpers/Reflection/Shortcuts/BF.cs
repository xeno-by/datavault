using System.Reflection;

namespace DataVault.Core.Helpers.Reflection.Shortcuts
{
    public class BF
    {
        public const BindingFlags All = Public | Private;
        public const BindingFlags DeclaredOnly = BindingFlags.DeclaredOnly;

        public const BindingFlags Public = PublicInstance | PublicStatic;
        public const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        public const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        public const BindingFlags Private = PrivateInstance | PrivateStatic;
        public const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    }
}