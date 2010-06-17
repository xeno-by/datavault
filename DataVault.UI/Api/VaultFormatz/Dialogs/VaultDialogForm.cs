using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.UI.Api.VaultFormatz.Dialogs
{
    public partial class VaultDialogForm : Form, IVaultDialog
    {
        public VaultAction Action { get; private set; }
        public IVault SelectedVault { get; private set; }

        public VaultDialogForm(VaultAction action)
        {
            // Initialize internal state
            Action = action;
            SelectedVault = null;

            // Initialize controls and events
            InitializeComponent();
            _tcTabs.Selected += (o, e) =>
            {
                var virtualTab = e.TabPage.Tag.AssertCast<IVaultDialogTab>();
                this.Text = virtualTab.FullTitle;
                if (TabActivated != null)
                    TabActivated(o, new VaultDialogTabEventArgs(virtualTab));
            };
            _tcTabs.Deselected += (o, e) =>
            {
                var virtualTab = e.TabPage.Tag.AssertCast<IVaultDialogTab>();
                if (TabDeactivated != null)
                    TabDeactivated(o, new VaultDialogTabEventArgs(virtualTab));
            };

            // Load plugins
            var lastSelectedTabType = (Type)AppDomain.CurrentDomain.GetData(this.GetType().FullName);
            lastSelectedTabType = lastSelectedTabType ?? VaultFormats.Default.GetType();
            TabPage tabToSelectNow = null;

            var map = new Dictionary<TabPage, IVaultFormat>();
            foreach (var format in VaultFormats.All)
            {
                format.InjectIntoVaultDialog(this);

                var tab = _tcTabs.TabPages.Cast<TabPage>().Last();
                map.Add(tab, format);

                if (format.GetType() == lastSelectedTabType)
                {
                    tabToSelectNow = tab;
                }
            }

            // Select previously selected page (or a default format)
            tabToSelectNow.AssertNotNull();
            if (tabToSelectNow != null)
            {
                if (tabToSelectNow == _tcTabs.TabPages.Cast<TabPage>().First())
                {
                    // the target tab is already selected, but we need to raise our events
                    if (TabActivated != null)
                        TabActivated(this, new VaultDialogTabEventArgs(Tabs.First()));
                }
                else
                {
                    _tcTabs.SelectedTab = tabToSelectNow;
                }
            }

            // Remember tab selections
            this.TabActivated += (o, e) =>
            {
                var tabType = map[e.Tab.TabPage].GetType();
                AppDomain.CurrentDomain.SetData(this.GetType().FullName, tabType);
            };
        }

        public event EventHandler<VaultDialogTabEventArgs> TabActivated;
        public event EventHandler<VaultDialogTabEventArgs> TabDeactivated;

        public class VaultDialogTab : IVaultDialogTab
        {
            public Guid Id { get; private set; }
            public TabPage TabPage { get; private set; }
            public TabControl TabControl { get { return (TabControl)TabPage.Parent; } }
            public VaultDialogForm Form { get { return (VaultDialogForm)TabControl.Parent.Parent; } }

            public VaultDialogTab(TabPage winformsTab)
            {
                Id = Guid.NewGuid();
                TabPage = winformsTab;
            }

            public string ShortTitle
            {
                get { return TabPage.Text; }
                set { TabPage.Text = value; }
            }

            private String _fullTitle;
            public string FullTitle
            {
                get { return _fullTitle; }
                set
                {
                    _fullTitle = value;
                    if (TabControl.SelectedTab == TabPage)
                    {
                        Form.Text = value;
                    }
                }
            }
        }

        public ReadOnlyCollection<IVaultDialogTab> Tabs
        {
            get
            {
                return new ReadOnlyCollection<IVaultDialogTab>(
                    _tcTabs.TabPages.Cast<TabPage>().Select(p => (IVaultDialogTab)p.Tag).ToList());
            }
        }

        public IVaultDialogTab ActiveTab
        {
            get { return (IVaultDialogTab)_tcTabs.SelectedTab.Tag; }
        }

        public IVaultDialogTab AddTab(string shortTitle, string fullTitle)
        {
            _tcTabs.TabPages.Add(String.Empty);
            var winformsTab = _tcTabs.TabPages.Cast<TabPage>().Last();

            var virtualTab = new VaultDialogTab(winformsTab);
            virtualTab.ShortTitle = shortTitle;
            virtualTab.FullTitle = fullTitle;
            winformsTab.Tag = virtualTab;

            return virtualTab;
        }

        private bool closingFromEndDialog = false;
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (closingFromEndDialog)
            {
                return;
            }
            else
            {
                SelectedVault = null;
                DialogResult = DialogResult.Cancel;
            }
        }

        public void EndDialog()
        {
            SelectedVault = null;
            DialogResult = DialogResult.Cancel;
            closingFromEndDialog = true;
            Close();
        }

        public void EndDialog(IVault vault)
        {
            SelectedVault = vault;
            DialogResult = vault == null ? DialogResult.Cancel : DialogResult.OK;
            closingFromEndDialog = true;
            Close();
        }

        private void VaultDialogForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
            {
                EndDialog(null);
            }
        }
    }
}
