using System;
using System.Linq;
using DataVault.Core.Helpers.Reflection.Shortcuts;

namespace DataVault.UI.Api.VaultViewz
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class VaultViewLocAttribute : Attribute
    {
        public Type ResourcesClass { get; private set; }

        private String NameKey { get; set; }
        public String Name
        {
            get
            {
                return (String)ResourcesClass.GetProperties(BF.All)
                   .Single(p => p.Name == NameKey).GetValue(null, null);
            }
        }

        public VaultViewLocAttribute(Type resourcesClass, String nameKey)
        {
            ResourcesClass = resourcesClass;
            NameKey = nameKey;
        }
    }
}