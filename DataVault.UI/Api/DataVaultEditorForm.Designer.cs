using DataVault.UI.Impl.Api;

namespace DataVault.UI.Api
{
    partial class DataVaultEditorForm
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
                _editor.Dispose();
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
            this._editor = new DataVaultEditor();
            this.SuspendLayout();
            // 
            // _editor
            // 
            this._editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this._editor.Location = new System.Drawing.Point(0, 0);
            this._editor.Name = "_editor";
            this._editor.Size = new System.Drawing.Size(912, 627);
            this._editor.TabIndex = 0;
            // 
            // DataVaultEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(912, 627);
            this.Controls.Add(this._editor);
            this.Name = "DataVaultEditorForm";
            this.Text = "dataVaultEditorForm";
            this.ResumeLayout(false);

        }

        #endregion

        internal DataVaultEditor _editor;
    }
}