using System;
using System.Windows.Forms;
using DataVault.UI.Api.Commands.WithExecutor;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;
using DataVault.UI.Utils.CustomMessageBoxes;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class VaultExitCommand : ContextBoundCommand
    {
        public VaultExitCommand(DataVaultUIContext context)
            : base(context)
        {
        }

        public override void DoImpl()
        {
            MakeSureItsSafeToCloseTheVault();
            Application.Exit();
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}