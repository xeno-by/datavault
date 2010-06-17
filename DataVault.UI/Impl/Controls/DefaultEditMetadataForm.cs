using System;
using System.ComponentModel;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Commands;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;
using DataVault.Core.Helpers;
using System.Linq;
using DataVault.UI.Properties;

namespace DataVault.UI.Impl.Controls
{
    public partial class DefaultEditMetadataForm : Form
    {
        private DataVaultUIContext DataVaultUIContext { get; set; }
        private IElement Element { get; set; }
        private IMetadata Metadata { get; set; }

        public DefaultEditMetadataForm(DataVaultUIContext context, IElement element)
        {
            DataVaultUIContext = context;
            Element = element.AssertNotNull();

            InitializeComponent();
            _tbName.Text = element.Name.ResolveIfSpecial();
            _cbElementType.SelectedIndex = element is IBranch ? 0 : 1;

            _tcMetadata.TabPages.Clear();
            var @default = element.Metadata.Where(kvp => kvp.Key == "default");
            var ordered = @default.Concat(element.Metadata.Except(@default));
            ordered.ForEach(kvp => AddSection(kvp.Key, kvp.Value));

            Action recalcEnabled = () => _sectionDeletePopup.Enabled =
                _tcMetadata.TabPages.Count > 0 &&
                ((TextBox)_tcMetadata.TabPages[_tcMetadata.SelectedIndex].Controls.Find("_tbName", true).Single()).Enabled;
            recalcEnabled();
            _tcMetadata.ControlAdded += (o, args) => recalcEnabled();
            _tcMetadata.ControlRemoved += (o, args) => recalcEnabled();
            _tcMetadata.SelectedIndexChanged += (o, args) => { recalcEnabled(); FocusContentControl(); };
        }

        private void _sectionAddPopup_Click(object sender, EventArgs e)
        {
            Func<int, String> namegen = i => String.Format(Resources.New_MetadataSectionDefaultName, i);
            var lastUsedIndex = 1.Seq(i => i + 1, i => _tcMetadata.TabPages.Cast<TabPage>().Any(tp => tp.Name == namegen(i))).LastOrDefault();
            var firstUnusedName = namegen(lastUsedIndex + 1);
            AddSection(firstUnusedName, String.Empty);
        }

        private void _sectionDeletePopup_Click(object sender, EventArgs e)
        {
            var currentTab = _tcMetadata.SelectedIndex;
            _tcMetadata.TabPages.RemoveAt(currentTab);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            FocusContentControl();
        }

        private void FocusContentControl()
        {
            var currentTab = _tcMetadata.TabPages[_tcMetadata.SelectedIndex];
            var tbContent = (TextBox)currentTab.Controls.Find("_tbContent", true).Single();
            tbContent.Focus();
            tbContent.Select(0, 0);
        }

        private void DefaultEditMetadataForm_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !e.Alt && !e.Control && !e.Shift)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }

            if (e.KeyCode == Keys.Enter && !e.Alt && e.Control && !e.Shift)
            {
                DialogResult = DialogResult.OK;
                Close();
            }

            if (e.KeyCode == Keys.F4 && !e.Alt && e.Control && !e.Shift)
            {
                _sectionDeletePopup_Click(this, EventArgs.Empty);
            }
        }

        private void AddSection(String name, String content)
        {
            _tcMetadata.TabPages.Add(name, name);
            var tp = _tcMetadata.TabPages.Cast<TabPage>().Last();
            tp.Text = tp.Text.ResolveIfSpecial();

            var tbName = new TextBox { Name = "_tbName", Text = name, Dock = DockStyle.Fill };
            tbName.TextChanged += (o, e) => tp.Text = tbName.Text;
            tbName.Enabled = name == tp.Text;

            var tbContent = new TextBox { Name = "_tbContent", Text = content, Dock = DockStyle.Fill };
            tbContent.Multiline = true;
            tbContent.ScrollBars = ScrollBars.Both;

            var p1 = new Panel { Dock = DockStyle.Top, Height = tbName.Height + 10};
            var p11 = new Panel { Dock = DockStyle.Left, Width = 75 };
            var p12 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5, 5, 5, 5) };
            var p2 = new Panel { Dock = DockStyle.Fill };
            var p21 = new Panel { Dock = DockStyle.Left, Width = 75 };
            var p22 = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5, 5, 5, 5) };

            tp.Controls.Add(p2);
            tp.Controls.Add(p1);
            p1.Controls.Add(p12);
            p1.Controls.Add(p11);
            p2.Controls.Add(p22);
            p2.Controls.Add(p21);

            p11.Controls.Add(new Label { Text = Resources.DefaultEditMetadataForm_SectionTab_Name, Padding = new Padding(8, 8, 0, 0) });
            p12.Controls.Add(tbName);
            p21.Controls.Add(new Label { Text = Resources.DefaultEditMetadataForm_SectionTab_Content, Padding = new Padding(8, 8, 0, 0) });
            p22.Controls.Add(tbContent);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (DialogResult == DialogResult.OK)
            {
                Metadata = Element.Metadata.Clone();
                Metadata.Keys.ForEach(k => Metadata.Remove(k));

                foreach (TabPage tp in _tcMetadata.TabPages)
                {
                    var tbName = tp.Controls.Find("_tbName", true).Single();
                    var tbContent = tp.Controls.Find("_tbContent", true).Single();
                    Metadata[tbName.Text] = tbContent.Text;
                }
            }
        }

        public ICommand IssueApplyChangesCommand()
        {
            if (Metadata == null)
            {
                return null;
            }
            else
            {
                if (Element is IValue)
                {
                    return new ValueEditMetadataFinishCommand(DataVaultUIContext, Metadata);
                }
                else
                {
                    (Element is IBranch).AssertTrue();
                    return new BranchEditMetadataFinishCommand(DataVaultUIContext, Metadata);
                }
            }
        }
    }
}
