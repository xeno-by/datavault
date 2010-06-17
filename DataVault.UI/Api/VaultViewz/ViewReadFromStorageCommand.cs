using System;
using System.IO;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Api.VaultViewz
{
    [GhostableInHistory]
    internal class ViewReadFromStorageCommand<T> : ContextBoundCommand
    {
        private readonly IVaultView _view;
        private readonly String _vpath;
        public T Result { get; private set; }

        public ViewReadFromStorageCommand(DataVaultUIContext context, IVaultView view, String vpath)
            : base(context)
        {
            _view = view;
            _vpath = vpath;

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
                Result = (T)(Object)cfgEntry.ContentStream.CacheInMemory();
            }
            else
            {
                Result = cfgEntry.ContentString.FromInvariantString<T>();
            }
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}