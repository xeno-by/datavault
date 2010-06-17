using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Exceptions;
using DataVault.UI.Commands;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Properties;
using DataVault.UI.Api.ContentTypez;
using DataVault.Core.Helpers;
using System.Linq;

namespace DataVault.UI.Impl.Controls
{
    internal partial class DefaultEditValueForm : Form
    {
        private DataVaultUIContext DataVaultUIContext { get; set; }
        private IValue Value { get; set; }
        private IContentTypeAppliedToValue Wrapper { get; set; }

        private new Size DefaultSize { get; set; }
        private Size InflatedSize { get; set; }

        public DefaultEditValueForm(DataVaultUIContext context, IValue value)
        {
            DataVaultUIContext = context.AssertNotNull();
            Value = value.AssertNotNull();
            Wrapper = value.GetTypeToken2() == "binary" ? null : ContentTypes.ApplyCType(value);

            InitializeComponent();
            DefaultSize = new Size(Size.Width, Size.Height - _panelBinary.Height);
            InflatedSize = new Size(DefaultSize.Width + 300, DefaultSize.Height + 400);

            _cbType.Items.Add(Resources.ValueType_NotSelected);
            ContentTypes.All.ForEach(w => _cbType.Items.Add(w.LocTypeName));
            _cbType.Items.Add(Resources.ValueType_Binary);
            if (Wrapper is UnknownContentTypeAppliedToValue) _cbType.Items.Add(Value.LocalizedTypeToken());
            _cbType.SelectedIndex = 0;

            _tbName.Text = Value.Name.ResolveIfSpecial();
            _cbType.SelectedItem = Value.LocalizedTypeToken();

            _tbValueTyped.Text = Wrapper == null ? null : Wrapper.AsLocalizedString;
            _tbValueBinary.Text = Resources.BinaryType_ContentNotEditedYet;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _tbValueTyped.Focus();
        }

        private void _cbType_SelectedIndexChanged(Object sender, EventArgs e)
        {
            var selected = (String)_cbType.SelectedItem;
            var isBinary = selected == Resources.ValueType_Binary;
            var isText = selected == Resources.ValueType_Text;

            _panelTyped.Visible = !isBinary;
            _panelBinary.Visible = isBinary;
            _panelTyped.Dock = DockStyle.Fill;
            _panelBinary.Dock = DockStyle.Fill;
            this.Size = isText ? InflatedSize : DefaultSize;

            if (isBinary)
            {
                _tbValueBinary.Text = Resources.BinaryType_ContentNotEditedYet;
            }
            else
            {
                _tbValueTyped.Multiline = isText;
                _tbValueTyped.ScrollBars = isText ? ScrollBars.Both : ScrollBars.None;
            }
        }

        private void _btnClearBinary_Click(Object sender, EventArgs e)
        {
            _tbValueBinary.Text = Resources.BinaryType_ContentCleared;
        }

        private void _btnSelectBinary_Click(Object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.ValidateNames = true;
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _tbValueBinary.Text = ofd.FileNames.Single();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                var selected = (String)_cbType.SelectedItem;
                var isNotSelected = selected == Resources.ValueType_NotSelected;
                var isBinary = selected == Resources.ValueType_Binary;

                if (isNotSelected)
                {
                    MessageBox.Show(Resources.Validation_ValueTypeNotSelected, Resources.Validation_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
                else
                {
                    if (isBinary)
                    {
                        if (_tbValueBinary.Text == Resources.BinaryType_ContentNotEditedYet ||
                            _tbValueBinary.Text == Resources.BinaryType_ContentCleared)
                        {
                            // valid
                        }
                        else
                        {
                            try
                            {
                                using(File.OpenRead(_tbValueBinary.Text)){}
                            }
                            catch(Exception)
                            {
                                MessageBox.Show(Resources.BinaryType_ErrorOpeningContentFile, Resources.Validation_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                e.Cancel = true;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            // need to reuse the Wrapper because it might represent an unknown ctype
                            // in that case ContentTypes.All won't contain it and we won't be able to succeed
                            if (Wrapper != null && Wrapper.CType.LocTypeName == (String)_cbType.SelectedItem)
                            {
                                Wrapper.AssertNotNull();
                                Wrapper.AsLocalizedString = _tbValueTyped.Text;
                            }
                            else
                            {
                                // we're checking this case to make sure that changing ctypes works fine
                                var wrapper_t = ContentTypes.All.Single(t => t.LocTypeName == (String)_cbType.SelectedItem);
                                var typedWrapper = wrapper_t.Apply(Value);
                                typedWrapper.AsLocalizedString = _tbValueTyped.Text;
                            }
                        }
                        catch (ValidationException vex)
                        {
                            MessageBox.Show(vex.Message, Resources.Validation_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                        }
                    }
                }
            }

            base.OnClosing(e);
        }

        private void EditValueForm_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && !e.Alt && !e.Control && !e.Shift)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }

            if (e.KeyCode == Keys.Enter && !e.Alt && !e.Control && !e.Shift && 
                (String)_cbType.SelectedItem != "text")
            {
                DialogResult = DialogResult.OK;
                Close();
            }

            if (e.KeyCode == Keys.Enter && !e.Alt && e.Control && !e.Shift)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        public ICommand IssueApplyChangesCommand()
        {
            var selected = (String)_cbType.SelectedItem;
            var isNotSelected = selected == Resources.ValueType_NotSelected;
            var isBinary = selected == Resources.ValueType_Binary;

            if (isNotSelected)
            {
                return null;
            }
            else
            {
                if (isBinary)
                {
                    var bival = _tbValueBinary.Text;
                    if (bival == Resources.BinaryType_ContentNotEditedYet)
                    {
                        return null;
                    }
                    else if (bival == Resources.BinaryType_ContentCleared)
                    {
                        return new ValueEditBinaryFinishCommand(DataVaultUIContext, String.Empty.AsStream());
                    }
                    else
                    {
                        try
                        {
                            using(var fs = File.OpenRead(_tbValueBinary.Text))
                            {
                                return new ValueEditBinaryFinishCommand(DataVaultUIContext, fs.CacheInMemory());
                            }
                        }
                        catch(Exception)
                        {
                            MessageBox.Show(Resources.BinaryType_ErrorOpeningContentFile, Resources.Validation_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                    }
                }
                else
                {
                    if (Wrapper != null && Wrapper.CType.LocTypeName == (String)_cbType.SelectedItem)
                    {
                        Wrapper.AssertNotNull();
                        Wrapper.AsLocalizedString = _tbValueTyped.Text;
                        return Wrapper.IssueApplyChangesCommand(DataVaultUIContext);
                    }
                    else
                    {
                        // we're checking this case to make sure that binary -> w/e works fine
                        var wrapper_t = ContentTypes.All.Single(t => t.LocTypeName == (String)_cbType.SelectedItem);
                        var typedWrapper = wrapper_t.Apply(Value);
                        typedWrapper.AsLocalizedString = _tbValueTyped.Text;
                        return typedWrapper.IssueApplyChangesCommand(DataVaultUIContext);
                    }
                }
            }
        }
    }
}