using System;

namespace DataVault.Core.Impl
{
    [Flags]
    internal enum ValueKind
    {
        Regular = 1,
        Internal = 2,
        RegularAndInternal = 3
    }
}