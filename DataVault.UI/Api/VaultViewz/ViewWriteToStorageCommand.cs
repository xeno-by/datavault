using System;
using System.IO;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.VaultViewz
{
    [GhostableInHistory]
    internal class ViewWriteToStorageCommand<T> : ContextBoundCommand
    {
        private readonly IVaultView _view;
        private readonly String _vpath;
        private readonly T _value;

        public ViewWriteToStorageCommand(DataVaultUIContext context, IVaultView view, String vpath, T value)
            : base(context)
        {
            _view = view;
            _vpath = vpath;
            _value = value;

            var isStream = typeof(Stream).IsAssignableFrom(typeof(T));
            var hasBijectionWithString = typeof(T).SupportsSerializationToString();
            (isStream || hasBijectionWithString).AssertTrue();
        }

        public override void DoImpl()
        {
            var cfgRoot = Ctx.Vault.GetOrCreateBranch("$viewscfg");
            var viewCfg = cfgRoot.GetOrCreateBranch(_view.Name);
            var cfgEntry = viewCfg.GetOrCreateValue(_vpath, ((String)null).AsLazyStream());

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                var cached = ((Stream)(Object)_value).CacheInMemory();
                cfgEntry.SetContent(() => cached);
            }
            else
            {
                var content = _value.ToInvariantString();
                cfgEntry.SetContent(content.AsLazyStream());
            }
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}