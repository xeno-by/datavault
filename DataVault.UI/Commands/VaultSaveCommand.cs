using System;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class VaultSaveCommand : ContextBoundCommand
    {
        public VaultSaveCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Vault != null && Ctx.IsDirty;
        }

        public override void DoImpl()
        {
            Vault.Save();
            Ctx.MarkNotDirty();
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}