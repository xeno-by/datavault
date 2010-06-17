using System;

namespace DataVault.Core.Helpers.Collections
{
    [Flags]
    public enum ItemListChangeAction
    {
        Add = 1,
        Remove = 2,
        Replace = 3
    }
}