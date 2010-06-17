using System;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Impl.Memory;
using DataVault.Core.Helpers;

namespace DataVault.Core.Impl.Virtual
{
    // todo. implement smart virtualization (track changed stuff and save only it)
    // the stuff above also means: do not cache everything before disposing an old vault

    internal class VirtualVault : InMemoryVault
    {
        private readonly IVault _vault;
        private readonly IDisposable _vaultExposition;
        private readonly VaultVisitor _virtualizer;
        private readonly VaultVisitor _materializer;

        public override string Uri { get { return _vault.Uri; } }

        public VirtualVault(IVault vault, VaultVisitor virtualizer, VaultVisitor materializer)
        {
            _vault = vault.AssertNotNull();
            _virtualizer = virtualizer.AssertNotNull();
            _materializer = materializer.AssertNotNull();

            _vaultExposition = _vault.ExposeReadOnly();
            _virtualizer.Visit(this, _vault);
        }

        private void Materialize()
        {
            using (_vault.ExposeReadWrite())
            {
                var copyCat = VaultApi.OpenInMemory();
                _materializer.Visit(copyCat, this);

                _vault.Root.Delete();
                _vault.ImportFrom(copyCat);
            }
        }

        public override IVault Save()
        {
            Materialize();
            _vault.Save();
            return this;
        }

        public override IVault SaveAs(string uri)
        {
            Materialize();
            _vault.SaveAs(uri);
            return this;
        }

        public override IVault Backup()
        {
            Materialize();
            _vault.Backup();
            return this;
        }

        public override void Dispose()
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    _vaultExposition.Dispose();
                    base.Dispose();
                }
            } 
        }
    }
}
