using System;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers.Reflection;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api;

namespace DataVault.UI.Api.VaultViewz
{
    public abstract class VaultView : IVaultView
    {
        protected DataVaultUIContext _ctx;

        public string Name
        {
            get { return this.GetType().Attr<VaultViewAttribute>().Name; }
        }

        public string LocName
        {
            get { return this.GetType().Attr<VaultViewLocAttribute>().Name;  }
        }

        public void Apply(DataVaultUIContext ctx)
        {
            _ctx.AssertNull();
            ctx.AssertNotNull();

            _ctx = ctx;
            ApplyImpl();
        }

        protected virtual void ApplyImpl()
        {
            // do nothing by default
        }

        public void Dispose()
        {
            Discard();
        }

        public void Discard()
        {
            _ctx.AssertNotNull();
            DiscardImpl();
            _ctx = null;
        }

        protected virtual void DiscardImpl()
        {
            // do nothing by default
        }

        public virtual T ReadFromStorage<T>(String vpath)
        {
            var cmd = new ViewReadFromStorageCommand<T>(_ctx, this, vpath);
            _ctx.Execute(cmd);
            return cmd.Result;
        }

        public virtual void WriteToStorage<T>(String vpath, T value)
        {
            var cmd = new ViewWriteToStorageCommand<T>(_ctx, this, vpath, value);
            _ctx.Execute(cmd);
        }
    }
}