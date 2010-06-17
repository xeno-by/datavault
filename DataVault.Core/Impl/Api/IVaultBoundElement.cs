using DataVault.Core.Api;

namespace DataVault.Core.Impl.Api
{
    internal interface IVaultBoundElement : IElement
    {
        void Bind(VaultBase vault);
        void Unbind();
    }
}