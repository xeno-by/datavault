using System;
using System.IO;
using DataVault.Core.Api;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    public abstract class ValueEditFinishCommand : ValueRelatedContextBoundCommand
    {
        private String OldTypeToken { get; set; }
        private String OldContent { get; set; }

        protected ValueEditFinishCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        protected abstract String NewTypeToken { get; }
        protected abstract Stream NewContent { get; }

        public override void DoImpl()
        {
            OldTypeToken = Value.GetTypeToken2();
            OldContent = Value.ContentString;

            Value.SetTypeToken2(NewTypeToken);
            Value.SetContent(NewContent);
            RefreshListAndThenSelect(Value);
        }

        public override void UndoImpl()
        {
            Value.SetTypeToken2(OldTypeToken);
            Value.SetContent(OldContent);
            RefreshListAndThenSelect(Value);
        }
    }
}