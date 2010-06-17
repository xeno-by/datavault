using System;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.VaultViewz
{
    public interface IVaultView : IDisposable
    {
        String Name { get; }
        String LocName { get; }

        void Apply(DataVaultUIContext ctx);
        void Discard();
    }
}
