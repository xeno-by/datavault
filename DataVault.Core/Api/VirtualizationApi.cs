using System;
using DataVault.Core.Impl.Virtual;

namespace DataVault.Core.Api
{
    public static class VirtualizationApi
    {
        public static IVault Virtualize(this IVault @this, VaultVisitor virtualizer, VaultVisitor materializer)
        {
            return new VirtualVault(@this, virtualizer, materializer);
        }
    }
}