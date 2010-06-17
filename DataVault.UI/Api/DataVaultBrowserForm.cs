using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.UI.Api.Versioning;
using DataVault.UI.Commands;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;
using DataVault.UI.Api.VaultFormatz;

namespace DataVault.UI.Api
{
    public partial class DataVaultBrowserForm : Form
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<DataVaultUIContext, IElement, bool> Approver { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IElement SelectedElement { get; private set; }

        // is necessary for default selector
        private Control _lastFocused;

        public DataVaultBrowserForm()
            : this(VaultFormats.Default, null)
        {
        }

        public DataVaultBrowserForm(IVaultFormat format)
            : this(format, null)
        {
        }

        public DataVaultBrowserForm(String uri)
            : this(VaultFormats.Default, uri)
        {
        }

        public DataVaultBrowserForm(IVaultFormat format, String uri)
        {
            Initialize();
            DeferredVaultInitialization = () =>
            {
                var fmt = format ?? VaultFormats.Default;
                if (uri == null)
                {
                    if (!_browser.Ctx.Execute(new VaultOpenCommand(_browser.Ctx)))
                    {
                        DialogResult = DialogResult.Cancel;
                        Close();
                    }
                }
                else
                {
                    var vault = fmt.OpenCore(uri);
                    _browser.Ctx.SetVault(vault, false);
                }
            };
        }

        public DataVaultBrowserForm(IVault externalVault)
        {
            Initialize();
            DeferredVaultInitialization = () => _browser.Ctx.LeaseVault(externalVault, false);
        }

        private void Initialize()
        {
            InitializeComponent();
            _browser.Ctx.AutoSaveEnabled = false;
            Approver = (ctx, el) => true;

            _browser._tree.GotFocus += (o, e) => _lastFocused = _browser._tree;
            _browser._list.GotFocus += (o, e) => _lastFocused = _browser._list;

            _browser.TextChanged += (o, e) => 
            {
                var fname = _browser.Ctx.Vault == null ? Resources.MainForm_Title_NA : _browser.Ctx.Vault.Uri;
                var moduleVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var structureVersion = StructureVersion.Current;
                this.Text = String.Format(Resources.BrowserForm_Title, moduleVersion, structureVersion, fname);
            };

            _browser.AfterUIResync += (o, e) =>
            {
                var tree = _browser._tree;
                var list = _browser._list;

                IElement el = null;
                if (_lastFocused == tree) el = tree.SelectedNode == null ? null : (IBranch)tree.SelectedNode.Tag;
                if (_lastFocused == list) el = list.SelectedItems.Count == 0 ? null : (IValue)list.SelectedItems[0].Tag;

                var approved = Approver(_browser.Ctx, el);
                SelectedElement = approved ? el : null;
                _okButton.Enabled = approved;
            };

            _browser._tree.MouseDoubleClick += (o, e) =>
            {
                if (e.Clicks == 2 && e.Button == MouseButtons.Left)
                    _okButton_Click(this, EventArgs.Empty);
            };

            _browser._list.MouseDoubleClick += (o, e) =>
            {
                if (e.Clicks == 2 && e.Button == MouseButtons.Left)
                    _okButton_Click(this, EventArgs.Empty);
            };
        }

        private Action DeferredVaultInitialization { get; set; }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DeferredVaultInitialization();
        }

        private void DataVaultBrowserForm_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Control && !e.Alt && !e.Shift)
            {
                _okButton_Click(this, EventArgs.Empty);
            }

            if (e.KeyCode == Keys.Escape && !e.Control && !e.Alt && !e.Shift)
            {
                _cancelButton_Click(this, EventArgs.Empty);
            }
        }

        private void _okButton_Click(Object sender, EventArgs e)
        {
            if (_okButton.Enabled)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void _cancelButton_Click(Object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (SelectedElement != null)
                SelectedElement.CacheInMemory();
        }
    }
}