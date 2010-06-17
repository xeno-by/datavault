namespace DataVault.UI.Impl.Controls
{
    partial class DefaultEditMetadataForm
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefaultEditMetadataForm));
            this._okButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel7 = new System.Windows.Forms.Panel();
            this._tbName = new System.Windows.Forms.TextBox();
            this.panel8 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this._cancelButton = new System.Windows.Forms.Button();
            this._cbElementType = new System.Windows.Forms.ComboBox();
            this.panel10 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this._metadataPopup = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._sectionAddPopup = new System.Windows.Forms.ToolStripMenuItem();
            this._sectionDeletePopup = new System.Windows.Forms.ToolStripMenuItem();
            this._tcMetadata = new System.Windows.Forms.TabControl();
            this.panel9 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel7.SuspendLayout();
            this.panel8.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel10.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this._metadataPopup.SuspendLayout();
            this.panel9.SuspendLayout();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this._okButton, "_okButton");
            this._okButton.Name = "_okButton";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.label1);
            resources.ApplyResources(this.panel7, "panel7");
            this.panel7.Name = "panel7";
            // 
            // _tbName
            // 
            resources.ApplyResources(this._tbName, "_tbName");
            this._tbName.Name = "_tbName";
            this._tbName.ReadOnly = true;
            // 
            // panel8
            // 
            this.panel8.Controls.Add(this._tbName);
            resources.ApplyResources(this.panel8, "panel8");
            this.panel8.Name = "panel8";
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.panel8);
            this.panel6.Controls.Add(this.panel7);
            resources.ApplyResources(this.panel6, "panel6");
            this.panel6.Name = "panel6";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label2);
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Name = "panel5";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this._cancelButton);
            this.panel1.Controls.Add(this._okButton);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this._cancelButton, "_cancelButton");
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _cbElementType
            // 
            resources.ApplyResources(this._cbElementType, "_cbElementType");
            this._cbElementType.Items.AddRange(new object[] {
            resources.GetString("_cbElementType.Items"),
            resources.GetString("_cbElementType.Items1")});
            this._cbElementType.Name = "_cbElementType";
            // 
            // panel10
            // 
            this.panel10.Controls.Add(this._cbElementType);
            resources.ApplyResources(this.panel10, "panel10");
            this.panel10.Name = "panel10";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.panel10);
            this.panel4.Controls.Add(this.panel5);
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.panel9);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // panel3
            // 
            this.panel3.ContextMenuStrip = this._metadataPopup;
            this.panel3.Controls.Add(this._tcMetadata);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // _metadataPopup
            // 
            this._metadataPopup.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._sectionAddPopup,
            this._sectionDeletePopup});
            this._metadataPopup.Name = "_metadataPopup";
            resources.ApplyResources(this._metadataPopup, "_metadataPopup");
            // 
            // _sectionAddPopup
            // 
            this._sectionAddPopup.Name = "_sectionAddPopup";
            resources.ApplyResources(this._sectionAddPopup, "_sectionAddPopup");
            this._sectionAddPopup.Click += new System.EventHandler(this._sectionAddPopup_Click);
            // 
            // _sectionDeletePopup
            // 
            this._sectionDeletePopup.Name = "_sectionDeletePopup";
            resources.ApplyResources(this._sectionDeletePopup, "_sectionDeletePopup");
            this._sectionDeletePopup.Click += new System.EventHandler(this._sectionDeletePopup_Click);
            // 
            // _tcMetadata
            // 
            this._tcMetadata.ContextMenuStrip = this._metadataPopup;
            resources.ApplyResources(this._tcMetadata, "_tcMetadata");
            this._tcMetadata.Name = "_tcMetadata";
            this._tcMetadata.SelectedIndex = 0;
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.label3);
            resources.ApplyResources(this.panel9, "panel9");
            this.panel9.Name = "panel9";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // DefaultEditMetadataForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel4);
            this.KeyPreview = true;
            this.Name = "DefaultEditMetadataForm";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DefaultEditMetadataForm_KeyDown);
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel10.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this._metadataPopup.ResumeLayout(false);
            this.panel9.ResumeLayout(false);
            this.panel9.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.TextBox _tbName;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.ComboBox _cbElementType;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl _tcMetadata;
        private System.Windows.Forms.ContextMenuStrip _metadataPopup;
        private System.Windows.Forms.ToolStripMenuItem _sectionAddPopup;
        private System.Windows.Forms.ToolStripMenuItem _sectionDeletePopup;
    }
}