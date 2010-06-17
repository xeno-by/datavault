using System;

namespace DataVault.Core.Helpers.Collections
{
    public delegate void ItemListChangeEventHandler<T>(Object sender, ItemListChangeEventArgs<T> e);
}