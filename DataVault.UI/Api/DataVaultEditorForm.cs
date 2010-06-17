using System;
using System.ComponentModel;
using System.Windows.Forms;
using DataVault.UI.Commands;

namespace DataVault.UI.Api
{
    public partial class DataVaultEditorForm : Form
    {
        public DataVaultEditorForm()
            : this(null)
        {
        }

        public DataVaultEditorForm(String initialUri)
        {
            InitializeComponent();
            _editor.Ctx.InitialUri = initialUri;
            _editor.TextChanged += (o, e) => this.Text = _editor.Text;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                var exit = new VaultExitCommand(_editor.Ctx);
                e.Cancel = !exit.CanDoImpl() || !_editor.Ctx.Execute(exit);
            }
        }
    }
}