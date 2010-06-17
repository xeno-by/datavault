using System;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.UIExtensionz
{
    public interface IDataVaultUIExtension : IDisposable
    {
        void Initialize(DataVaultUIContext ctx);
        void Uninitialize();
    }
}
