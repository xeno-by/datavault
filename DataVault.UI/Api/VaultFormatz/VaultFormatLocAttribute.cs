using System;
using System.Linq;
using DataVault.Core.Helpers.Reflection.Shortcuts;

namespace DataVault.UI.Api.VaultFormatz
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class VaultFormatLocAttribute : Attribute
    {
        public Type ResourcesClass { get; private set; }

        private String DialogTitleKey { get; set; }
        public String DialogTitle(VaultAction action)
        {
            return (String)ResourcesClass.GetProperties(BF.All)
                .Single(p => p.Name == DialogTitleKey + "_" + action.ToString()).GetValue(null, null);
        }

        private String TabTitleKey { get; set; }
        public String TabTitle(VaultAction action)
        {
            return (String)ResourcesClass.GetProperties(BF.All)
                .Single(p => p.Name == TabTitleKey + "_" + action.ToString()).GetValue(null, null);
        }

        public VaultFormatLocAttribute(Type resourcesClass, String dialogTitleKey, String tabTitleKey)
        {
            ResourcesClass = resourcesClass;
            DialogTitleKey = dialogTitleKey;
            TabTitleKey = tabTitleKey;
        }
    }
}