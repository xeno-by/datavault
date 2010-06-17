namespace DataVault.UI.Api
{
    partial class DataVaultBrowserForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                _browser.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataVaultBrowserForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this._browser = new DataVaultEditor();
            this.panel2 = new System.Windows.Forms.Panel();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AccessibleDescription = null;
            this.panel1.AccessibleName = null;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackgroundImage = null;
            this.panel1.Controls.Add(this._browser);
            this.panel1.Font = null;
            this.panel1.Name = "panel1";
            // 
            // _browser
            // 
            this._browser.AccessibleDescription = null;
            this._browser.AccessibleName = null;
            resources.ApplyResources(this._browser, "_browser");
            this._browser.BackgroundImage = null;
            this._browser.Font = null;
            this._browser.Name = "_browser";
            this._browser.ShowBranchPopup = false;
            this._browser.ShowMainMenu = false;
            this._browser.ShowValuePopup = false;
            this._browser.UseControlShortcuts = false;
            this._browser.UseMenuShortcuts = false;
            // 
            // panel2
            // 
            this.panel2.AccessibleDescription = null;
            this.panel2.AccessibleName = null;
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.BackgroundImage = null;
            this.panel2.Controls.Add(this._cancelButton);
            this.panel2.Controls.Add(this._okButton);
            this.panel2.Font = null;
            this.panel2.Name = "panel2";
            // 
            // _cancelButton
            // 
            this._cancelButton.AccessibleDescription = null;
            this._cancelButton.AccessibleName = null;
            resources.ApplyResources(this._cancelButton, "_cancelButton");
            this._cancelButton.BackgroundImage = null;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Font = null;
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
            // 
            // _okButton
            // 
            this._okButton.AccessibleDescription = null;
            this._okButton.AccessibleName = null;
            resources.ApplyResources(this._okButton, "_okButton");
            this._okButton.BackgroundImage = null;
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Font = null;
            this._okButton.Name = "_okButton";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this._okButton_Click);
            // 
            // DataVaultBrowserForm
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Font = null;
            this.Icon = null;
            this.KeyPreview = true;
            this.Name = "DataVaultBrowserForm";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DataVaultBrowserForm_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private DataVaultEditor _browser;
    }
}