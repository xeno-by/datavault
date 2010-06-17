using System;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.UIExtensionz
{
    public abstract class DataVaultUIExtension : IDataVaultUIExtension
    {
        protected DataVaultUIContext _ctx;

        public void Initialize(DataVaultUIContext ctx)
        {
            _ctx.AssertNull();
            ctx.AssertNotNull();

            _ctx = ctx;
            InitializeImpl();
        }

        protected virtual void InitializeImpl()
        {
            // do nothing by default
        }

        public void Uninitialize()
        {
            UninitializeImpl();
        }

        public void Dispose()
        {
            UninitializeImpl();
        }

        protected virtual void UninitializeImpl()
        {
            // do nothing by default
        }
    }
}