using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using DataVault.Core.Api;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.VaultFormatz;

namespace DataVault.UI.Api.ComponentModel
{
    public class ElementEditor : UITypeEditor
    {
        protected virtual IVaultFormat GetVaultFormat(ITypeDescriptorContext context, Object value)
        {
            var fmtProp = context.Instance == null ? null : context.Instance.GetType().GetProperty("VaultFormat");
            var fmtPropVal = fmtProp == null ? null : (String)fmtProp.GetValue(context.Instance, null);

            var fmtField = context.Instance == null ? null : context.Instance.GetType().GetField("VaultFormat");
            var fmtFieldVal = fmtField == null ? null : (String)fmtField.GetValue(context.Instance);

            var fmtSymbolicName = fmtPropVal ?? fmtFieldVal;
            return VaultFormats.All.SingleOrDefault(vf => vf.GetType().Name == fmtSymbolicName);
        }

        protected virtual String GetUri(ITypeDescriptorContext context, Object value)
        {
            var uriProp = context.Instance == null ? null : context.Instance.GetType().GetProperty("Uri");
            var uriPropVal = uriProp == null ? null : (String)uriProp.GetValue(context.Instance, null);

            var uriField = context.Instance == null ? null : context.Instance.GetType().GetField("Uri");
            var uriFieldVal = uriField == null ? null : (String)uriField.GetValue(context.Instance);

            return uriPropVal ?? uriFieldVal;
        }

        protected virtual IVault GetExternalVault(ITypeDescriptorContext context, Object value)
        {
            var vaultProp = context.Instance == null ? null : context.Instance.GetType().GetProperty("ExternalVault");
            var vaultPropVal = vaultProp == null ? null : (IVault)vaultProp.GetValue(context.Instance, null);

            var vaultField = context.Instance == null ? null : context.Instance.GetType().GetField("ExternalVault");
            var vaultFieldVal = vaultField == null ? null : (IVault)vaultField.GetValue(context.Instance);

            return vaultPropVal ?? vaultFieldVal;
        }

        protected virtual bool ApproveSelection(DataVaultUIContext context, IElement el)
        {
            return true;
        }

        public override Object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
        {
            if ((context != null) && (provider != null))
            {
                var svc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (svc != null)
                {
                    var externalVault = GetExternalVault(context, value);
                    if (externalVault == null)
                    {
                        var fmt = GetVaultFormat(context, value);
                        var uri = GetUri(context, value);

                        if (uri != null)
                        {
                            using (var browser = new DataVaultBrowserForm(fmt, uri))
                            {
                                browser.Approver = ApproveSelection;
                                if (svc.ShowDialog(browser) == DialogResult.OK)
                                {
                                    value = browser.SelectedElement;
                                }
                            }
                        }
                        else
                        {
                            // just do nothing - crash is an unsuitable behavior here
                        }
                    }
                    else
                    {
                        using (var browser = new DataVaultBrowserForm(externalVault))
                        {
                            browser.Approver = ApproveSelection;
                            if (svc.ShowDialog(browser) == DialogResult.OK)
                            {
                                value = browser.SelectedElement;
                            }
                        }
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return context != null ? UITypeEditorEditStyle.Modal : base.GetEditStyle(context);
        }
    }
}