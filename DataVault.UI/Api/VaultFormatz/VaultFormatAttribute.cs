using System;

namespace DataVault.UI.Api.VaultFormatz
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class VaultFormatAttribute : Attribute
    {
        public String Name { get; private set; }

        public VaultFormatAttribute(String name) 
        {
            Name = name;
        }
    }
}