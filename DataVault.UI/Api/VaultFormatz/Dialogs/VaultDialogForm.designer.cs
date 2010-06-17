using System.Windows.Forms;

namespace DataVault.UI.Api.VaultFormatz.Dialogs
{
    public partial class VaultDialogForm
    {
        private Panel panel1;
        private Button _btnOk;
        private Button _btnCancel;
        private TabControl _tcTabs;
        private Panel panel2;

        Button IVaultDialog.OkButton { get { return _btnOk; } }

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this._btnOk = new System.Windows.Forms.Button();
            this._btnCancel = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this._tcTabs = new System.Windows.Forms.TabControl();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this._btnOk);
            this.panel1.Controls.Add(this._btnCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 342);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(592, 35);
            this.panel1.TabIndex = 0;
            // 
            // _btnOk
            // 
            this._btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this._btnOk.Location = new System.Drawing.Point(437, 5);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 25);
            this._btnOk.TabIndex = 1;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this._btnCancel.Location = new System.Drawing.Point(512, 5);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 25);
            this._btnCancel.TabIndex = 0;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this._tcTabs);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(592, 342);
            this.panel2.TabIndex = 1;
            // 
            // _tcTabs
            // 
            this._tcTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tcTabs.Location = new System.Drawing.Point(0, 0);
            this._tcTabs.Name = "_tcTabs";
            this._tcTabs.SelectedIndex = 0;
            this._tcTabs.Size = new System.Drawing.Size(592, 342);
            this._tcTabs.TabIndex = 0;
            // 
            // VaultDialogForm
            // 
            this.ClientSize = new System.Drawing.Size(592, 377);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.KeyPreview = true;
            this.Name = "VaultDialogForm";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VaultDialogForm_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
