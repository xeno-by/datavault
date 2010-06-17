using System;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Reflection;

namespace DataVault.UI.Api.VaultViewz
{
    internal class VaultViewFactory : IVaultViewFactory
    {
        private readonly IVaultView _sample;
        private Func<IVaultView> _factory;

        public VaultViewFactory(Type t_view)
        {
            t_view.HasAttr<VaultViewAttribute>().AssertTrue();
            t_view.HasAttr<VaultViewLocAttribute>().AssertTrue();

            _factory = () => (IVaultView)Activator.CreateInstance(t_view);
            _sample = _factory();
        }

        public string Name
        {
            get { return _sample.Name; }
        }

        public string LocName
        {
            get { return _sample.LocName; }
        }

        public Type Type
        {
            get { return _sample.GetType(); }
        }

        public IVaultView Create()
        {
            return _factory();
        }
    }
}