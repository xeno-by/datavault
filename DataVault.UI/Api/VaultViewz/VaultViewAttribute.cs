using System;

namespace DataVault.UI.Api.VaultViewz
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class VaultViewAttribute : Attribute
    {
        public String Name { get; private set; }

        public VaultViewAttribute(String name) 
        {
            Name = name;
        }
    }
}