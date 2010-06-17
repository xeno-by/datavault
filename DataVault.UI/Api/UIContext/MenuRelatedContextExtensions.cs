using System;
using System.Windows.Forms;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.Commands.WithExecutor;

namespace DataVault.UI.Api.UIContext
{
    public static class MenuRelatedContextExtensions
    {
        public static MenuStrip MainMenu(this DataVaultUIContext ctx)
        {
            return ctx.DataVaultEditor._mainMenu;
        }

        public static ToolStripMenuItem VaultMenu(this DataVaultUIContext ctx)
        {
            return (ToolStripMenuItem)ctx.MainMenu().Items["_vault"];
        }

        public static ToolStripMenuItem EditMenu(this DataVaultUIContext ctx)
        {
            return (ToolStripMenuItem)ctx.MainMenu().Items["_edit"];
        }

        public static ToolStripMenuItem ViewsMenu(this DataVaultUIContext ctx)
        {
            return (ToolStripMenuItem)ctx.MainMenu().Items["_views"];
        }

        public static ToolStripMenuItem BranchMenu(this DataVaultUIContext ctx)
        {
            return (ToolStripMenuItem)ctx.MainMenu().Items["_branch"];
        }

        public static ContextMenuStrip BranchPopup(this DataVaultUIContext ctx)
        {
            return ctx.DataVaultEditor._branchPopup;
        }

        public static ToolStripMenuItem ValueMenu(this DataVaultUIContext ctx)
        {
            return (ToolStripMenuItem)ctx.MainMenu().Items["_value"];
        }

        public static ContextMenuStrip ValuePopup(this DataVaultUIContext ctx)
        {
            return ctx.DataVaultEditor._valuePopup;
        }

        public static ContextMenuStrip ValuePopup2(this DataVaultUIContext ctx)
        {
            return ctx.DataVaultEditor._valuePopup2;
        }

        public static void BindCommand(this DataVaultUIContext ctx, ToolStripMenuItem mi, Func<ICommand> fcmd)
        {
            ctx.DataVaultEditor._commands.Bind(mi, () => new ExecutorBoundWrapper<DataVaultUIContext>(ctx, fcmd()));
        }
    }
}