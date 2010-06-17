using DataVault.Core.Api;

namespace DataVault.Core.Impl.Api
{
    internal interface IContentAwareVault
    {
        void Bind(IBranch branch);
        void Bind(IValue value);

        void Unbind(IBranch branch);
        void Unbind(IValue value);
    }
}