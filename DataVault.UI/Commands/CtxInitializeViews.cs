using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DataVault.Core.Helpers;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.VaultViewz;
using DataVault.UI.Properties;

namespace DataVault.UI.Commands
{
    [GhostableInHistory]
    internal class CtxInitializeViews : ContextBoundCommand
    {
        public CtxInitializeViews(DataVaultUIContext context)
            : base(context)
        {
        }

        public override void DoImpl()
        {
            var presets = Ctx.Vault.Metadata["views"];
            if (presets.IsNeitherNullNorEmpty())
            {
                if (new Regex(@"^\[.*\]$").IsMatch(presets))
                {
                    var reversedViewNames = new List<String>();
                    if (presets != "[]") presets.Slice(1, -1).Split(',').Select(s => s.Trim()).ForEach(reversedViewNames.Add);
                    reversedViewNames.Reverse();

                    var supportedViews = VaultViewFactories.All;
                    var reversedPresetViews = reversedViewNames.Select(name => supportedViews.SingleOrDefault(v => v.Name == name));

                    if (reversedPresetViews.All(f => f != null))
                    {
                        reversedPresetViews.ForEach(f =>
                        {
                            var view = f.Create();
                            view.Apply(Ctx);
                            Ctx.Views.Push(view);
                        });
                    }
                    else
                    {
                        var unknownIndices = reversedPresetViews.Select((v, i) => v == null ? i : -1).Where(i => i != -1);
                        var unknownViews = unknownIndices.Select(i => reversedViewNames.ElementAt(i)).StringJoin();
                        var warningMessage = unknownIndices.Count() == 1 ?
                           String.Format(Resources.Views_UnknownSingle_Warning, unknownViews) :
                           String.Format(Resources.Views_UnknownMultiple_Warning, unknownViews);

                        if (MessageBox.Show(
                            warningMessage,
                            Resources.Confirmation_Title,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                        {
                            Ctx.SetVault(null, false);
                        }
                        else
                        {
                            reversedPresetViews.ForEach(f =>
                            {
                                if (f != null)
                                {
                                    var view = f.Create();
                                    view.Apply(Ctx);
                                    Ctx.Views.Push(view);
                                }
                            });

                            var viewStack = "[" + Views.Select(v1 => v1.Name).StringJoin() + "]";
                            Vault.Metadata["views"] = viewStack;
                            Vault.Save();
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show(
                        Resources.Views_CorruptedViewNote_Message,
                        Resources.Views_CorruptedViewNote_Title,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    {
                        Ctx.SetVault(null, false);
                    }
                    else
                    {
                        Vault.Metadata["views"] = null;
                        Vault.Save();
                    }
                }
            }
        }

        public override void UndoImpl()
        {
            throw new NotSupportedException();
        }
    }
}