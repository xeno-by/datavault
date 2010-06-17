using System;

namespace DataVault.UI.Api.VaultViewz
{
    public interface IVaultViewFactory
    {
        String Name { get; }
        String LocName { get; }

        Type Type { get; }
        IVaultView Create();
    }
}