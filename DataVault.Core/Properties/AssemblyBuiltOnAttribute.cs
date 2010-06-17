using System;
using System.Diagnostics;

namespace DataVault.Core.Properties
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    [DebuggerNonUserCode]
    internal class AssemblyBuiltOnAttribute : Attribute
    {
        public String MachineHash { get; set; }

        public AssemblyBuiltOnAttribute(){}
        public AssemblyBuiltOnAttribute(String machineHash) { MachineHash = machineHash; }
    }
}