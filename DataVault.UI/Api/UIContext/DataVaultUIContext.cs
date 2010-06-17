using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Collections;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Exceptions;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Api.VaultViewz;
using DataVault.UI.Commands;
using DataVault.UI.Properties;
using Timer = System.Threading.Timer;
using DataVault.UI.Api.Versioning;
using DataVault.UI.Impl.Helpers;

namespace DataVault.UI.Api.UIContext
{
    public class DataVaultUIContext : CommandExecutorWithHistory, IDisposable
    {
        private bool _commandsAllowed = true;
        public bool CommandsAllowed
        {
            get { return _commandsAllowed; }
            set { _commandsAllowed = value; }
        }

        private Timer _autoSaveTimer;
        private void EnableAutoSaveTimer()
        {
            if (_autoSaveTimer != null)
                DisableAutoSaveTimer();

            _autoSaveTimer = new Timer(obj =>
            {
                lock (this)
                {
                    if (this.Vault != null)
                    {
                        using (Vault.ExposeReadOnly())
                        {
                            this.Vault.Backup();
                        }
                    }
                }
            }, null, 0, 30000);
        }

        private void DisableAutoSaveTimer()
        {
            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.Dispose();
                _autoSaveTimer = null;
            }
        }

        private bool _autoSaveEnabled = true;
        public bool AutoSaveEnabled
        {
            get { return _autoSaveEnabled; }
            set
            {
                if (_autoSaveEnabled != value)
                {
                    _autoSaveEnabled = value;
                    if (_autoSaveEnabled)
                    {
                        EnableAutoSaveTimer();
                    }
                    else
                    {
                        DisableAutoSaveTimer();
                    }
                }
            }
        }

        private bool _isVaultExternal;
        public IVault Vault { get; private set; }

        private StackSlim<IVaultView> _views = new StackSlim<IVaultView>();
        public StackSlim<IVaultView> Views { get { return _views; } }

        public String InitialUri { get; internal set; }

        public void SetVault(IVault vault, bool enableAutoSave)
        {
            SetVaultImpl(vault, false, enableAutoSave);
        }

        public void LeaseVault(IVault vault, bool enableAutoSave)
        {
            SetVaultImpl(vault, true, enableAutoSave);
        }

        private void SetVaultImpl(IVault vault, bool isExternal, bool enableAutoSave)
        {
            if (Vault == vault)
            {
                return;
            }
            else
            {
                // this is essential for early catching of possible versioning issues
                vault.EnsureIsOfLatestStructureVersion();

                if (Vault != null)
                {
                    Views.ForEach(v => { v.Discard(); Views.Pop(); });
                    if (!_isVaultExternal) Vault.Dispose();
                    DisableAutoSaveTimer();
                }

                Vault = vault;
                _isVaultExternal = isExternal;

                _autoSaveEnabled = enableAutoSave;
                if (Vault != null && _autoSaveEnabled)
                {
                    EnableAutoSaveTimer();
                }

                ResetHistory();
                ValueInClipboard = null;
                BranchInClipboard = null;
                _dataVaultEditor.UpdateTitle();

                RebuildTreeNodes();
            }
        }

        private IBranch _branchInClipboard;
        public IBranch BranchInClipboard
        {
            get { return _branchInClipboard; }
            set { _branchInClipboard = value == null ? null : value.Clone(); }
        }

        private IValue _valueInClipboard;
        public IValue ValueInClipboard
        {
            get { return _valueInClipboard; }
            set { _valueInClipboard = value == null ? null : value.Clone(); }
        }

        private DataVaultEditor _dataVaultEditor;
        public DataVaultEditor DataVaultEditor
        {
            get
            {
                return _dataVaultEditor;
            }
        }

        public DataVaultUIContext(DataVaultEditor dataVaultEditor)
        {
            _dataVaultEditor = dataVaultEditor;
        }

        public void Dispose()
        {
            if (Vault != null)
            {
                Views.ForEach(v => { v.Discard(); Views.Pop(); });
                if (!_isVaultExternal) Vault.Dispose();
                DisableAutoSaveTimer();
            }
        }

#pragma warning disable 1911

        public override bool Execute(ICommand command)
        {
            return DataVaultEditor.ThreadSafeInvoke(() => base.Execute(command));
        }

        public override bool Unexecute(ICommand command)
        {
            return DataVaultEditor.ThreadSafeInvoke(() => base.Unexecute(command));
        }

#pragma warning restore 1911

        protected override bool DoExecute(ICommand command)
        {
            try
            {
                try
                {
                    DataVaultEditor.Cursor = Cursors.WaitCursor;
                    return base.DoExecute(command);
                }
                catch (ValidationException vex)
                {
                    MessageBox.Show(vex.Message, Resources.Validation_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                finally
                {
                    DataVaultEditor.Cursor = Cursors.Arrow;
                }
            }
            catch (Exception ex)
            {
                // todo. implement "copy exception" button
                MessageBox.Show(
                    String.Format(Resources.CommandFailed_Message, command.GetType().Name, ex),
                    Resources.CommandFailed_Title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (command is VaultExitCommand)
                {
                    if (MessageBox.Show(
                        Resources.ExitFailedDueToAnException_Message,
                        Resources.ExitFailedDueToAnException_Title,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                }

                return false;
            }
        }

        protected override bool DoUnexecute(ICommand command)
        {
            try
            {
                DataVaultEditor.Cursor = Cursors.WaitCursor;
                return base.DoUnexecute(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format(Resources.CommandFailed_Message, command.GetType().Name, ex),
                    Resources.CommandFailed_Title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                DataVaultEditor.Cursor = Cursors.Arrow;
            }
        }

        public TreeNode CreateTreeNodesRecursive(TreeNode parent, IBranch branch)
        {
            return DataVaultEditor.ThreadSafeInvoke(() =>
            {
                var nodesHost = parent == null ? DataVaultEditor._tree.Nodes : parent.Nodes;
                var current = nodesHost[nodesHost.Add(branch.AsUIElement())];
                branch.GetBranches().ForEach(b => CreateTreeNodesRecursive(current, b));
                return current;
            });
        }

        public void RebuildTreeNodes()
        {
            DataVaultEditor.ThreadSafeInvoke(() =>
            {
                DataVaultEditor._tree.BeginUpdate();
                DataVaultEditor._tree.Nodes.Clear();
                RebuildListItems();

                try
                {
                    if (Vault != null) CreateTreeNodesRecursive(null, Vault.Root);
                    var tree = DataVaultEditor._tree;
                    if (tree.Nodes.Count == 1) tree.SelectedNode = tree.Nodes[0];
                }
                finally
                {
                    DataVaultEditor._tree.EndUpdate();
                }
            });
        }

        public void RebuildListItems()
        {
            DataVaultEditor.ThreadSafeInvoke(() =>
            {

                DataVaultEditor._list.BeginUpdate();
                DataVaultEditor._list.Items.Clear();

                try
                {
                    var selectedBranch = DataVaultEditor._tree.SelectedNode == null ? null :
                       (IBranch)DataVaultEditor._tree.SelectedNode.Tag;
                    if (selectedBranch != null)
                    {
                        var sorted = selectedBranch.GetValues().Where(v => v.Name == "default").Concat(
                            selectedBranch.GetValues().Where(v => v.Name != "default"));
                        sorted.ForEach(v => DataVaultEditor._list.Items.Add(v.AsUIElement()));
                    }
                }
                finally
                {
                    DataVaultEditor._list.EndUpdate();
                }
            });
        }
    }
}