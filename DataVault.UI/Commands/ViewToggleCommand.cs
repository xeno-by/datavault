using System;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.Exceptions;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.VaultViewz;
using System.Linq;
using DataVault.UI.Properties;
using DataVault.Core.Helpers;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    public class ViewToggleCommand : ContextBoundCommand
    {
        private readonly String _viewName;

        public ViewToggleCommand(DataVaultUIContext context, String viewName)
            : base(context)
        {
            _viewName = viewName;
        }

        public override bool CanDoImpl()
        {
            return base.CanDoImpl() && Vault != null;
        }

        public override void DoImpl()
        {
            // menu itself will be updated and rearranged elsewhere (by the Editor control)

            if (Views.Any(v => v.Name == _viewName))
            {
                var top = Views.Peek();
                if (top.Name != _viewName)
                {
                    throw new ValidationException(Resources.Views_CanOnlyPopTopView_Message);
                }
                else
                {
                    var oldViewStack = Vault.Metadata["views"];
                    var newViewStack = "[" + Views.Skip(1).Select(v => v.Name).StringJoin() + "]";

                    try
                    {
                        // give view a chance to save this change
                        Vault.Metadata["views"] = newViewStack;

                        top.Discard();
                        Views.Pop();
                    }
                    catch (Exception)
                    {
                        Vault.Metadata["views"] = oldViewStack;
                        throw;
                    }
                }
            }
            else
            {
                var factory = VaultViewFactories.All.Single(v => v.Name == _viewName);
                var view = factory.Create();

                var oldViewStack = Vault.Metadata["views"];
                var newViewStack = "[" + view.MkArray().Concat(Views).Select(v => v.Name).StringJoin() + "]";

                try
                {
                    // give view a chance to save this change
                    Vault.Metadata["views"] = newViewStack;

                    view.Apply(Ctx);
                    Views.Push(view);
                }
                catch (Exception)
                {
                    Vault.Metadata["views"] = oldViewStack;
                    throw;
                }
            }
        }

        public override void UndoImpl()
        {
            try
            {
                DoImpl();
            }
            catch (ValidationException)
            {
                AssertionHelper.Fail();
            }
        }
    }
}
