using System;
using System.Diagnostics;

namespace DataVault.UI.Properties
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    [DebuggerNonUserCode]
    internal class AssemblyBuiltByAttribute : Attribute
    {
        public String UserHash { get; set; }

        public AssemblyBuiltByAttribute(){}
        public AssemblyBuiltByAttribute(String userHash) { UserHash = userHash; }
    }
}