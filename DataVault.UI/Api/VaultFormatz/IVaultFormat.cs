using System;
using DataVault.Core.Api;

namespace DataVault.UI.Api.VaultFormatz
{
    public interface IVaultFormat : IVaultFormatUI
    {
        String Name { get; }
        bool AcceptCore(String uri);
        IVault OpenCore(String uri);
    }
}