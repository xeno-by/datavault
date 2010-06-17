using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Api.Commands.WithUI;
using DataVault.UI.Api.ContentTypez;
using DataVault.UI.Api.UIContext;
using DataVault.UI.Api.UIExtensionz;
using DataVault.UI.Api.VaultFormatz;
using DataVault.UI.Api.VaultViewz;
using DataVault.UI.Api.Versioning;
using DataVault.UI.Commands;
using DataVault.UI.Api.Commands;
using DataVault.UI.Api.Commands.WithHistory;
using DataVault.UI.Properties;
using DataVault.Core.Helpers;
using System.Linq;

namespace DataVault.UI.Api
{
    public partial class DataVaultEditor : UserControl
    {
        #region Public API

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataVaultUIContext Ctx { get; private set; }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool ShowMainMenu
        {
            get { return _mainMenu.Visible; }
            set
            {
                _mainMenu.Visible = value;
                _mainMenuHost.Visible = value;

                if (value)
                {
                    if (!_mainMenuHost.Contains(_mainMenu))
                    {
                        _mainMenuHost.Controls.Add(_mainMenu);
                    }
                }
                else
                {
                    if (_mainMenuHost.Contains(_mainMenu))
                    {
                        _mainMenuHost.Controls.Remove(_mainMenu);
                    }
                }
            }
        }

        private bool _showBranchPopup = true;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool ShowBranchPopup
        {
            get { return _showBranchPopup; }
            set { _showBranchPopup = value; }
        }

        private bool _showValuePopup = true;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool ShowValuePopup
        {
            get { return _showValuePopup; }
            set { _showValuePopup = value; }
        }

        private bool _useControlShortcuts = true;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool UseControlShortcuts
        {
            get { return _useControlShortcuts; }
            set { _useControlShortcuts = value; }
        }

        private bool _useMenuShortcuts = true;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(true)]
        public bool UseMenuShortcuts
        {
            get { return _useMenuShortcuts; }
            set
            {
                if (_useMenuShortcuts != value)
                {
                    _useMenuShortcuts = value;

                    Action<ToolStrip> action;
                    if (value) action = menu => menu.EnshortcutAndRestore();
                    else action = menu => menu.UnshortcutAndStore();

                    action(_mainMenu);
                    action(_branchPopup);
                    action(_valuePopup);
                    action(_valuePopup2);
                }
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public event EventHandler BeforeUIResync;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public event EventHandler AfterUIResync;

        #endregion

        public DataVaultEditor()
            : this(null)
        {
        }

        public DataVaultEditor(String initialUri)
        {
            Ctx = new DataVaultUIContext(this);
            Ctx.InitialUri = initialUri;
            InitializeComponent();
        }

        internal readonly CommandMapping _commands = new CommandMapping();
        internal IDataVaultUIExtension[] _extensions;

        protected override void OnLoad(EventArgs args)
        {
            base.OnLoad(args);
            UpdateTitle();

            // Step 1. Bind apriori known actions

            Action<ToolStripItem, Func<ICommand>> bind = 
                (mi, fcmd) => Ctx.BindCommand((ToolStripMenuItem)mi, fcmd);

            bind(_vaultNew, () => new VaultNewCommand(Ctx));
            bind(_vaultOpen, () => new VaultOpenCommand(Ctx));
            bind(_vaultSave, () => new VaultSaveCommand(Ctx));
            bind(_vaultImport, () => new VaultImportCommand(Ctx));
            bind(_vaultExport, () => new VaultExportCommand(Ctx));
            bind(_vaultExit, () => new VaultExitCommand(Ctx));

            bind(_editUndo, () => new UndoCommand(Ctx));
            bind(_editRedo, () => new RedoCommand(Ctx));

            bind(_branchNew, () => new BranchNewCommand(Ctx));
            bind(_branchNewPopup, () => new BranchNewCommand(Ctx));
            bind(_branchEditMetadata, () => new BranchEditMetadataStartCommand(Ctx));
            bind(_branchEditMetadataPopup, () => new BranchEditMetadataStartCommand(Ctx));
            bind(_branchDelete, () => new BranchDeleteCommand(Ctx));
            bind(_branchDeletePopup, () => new BranchDeleteCommand(Ctx));
            bind(_branchRename, () => new BranchRenameStartCommand(Ctx));
            bind(_branchRenamePopup, () => new BranchRenameStartCommand(Ctx));
            bind(_branchMoveUp, () => new BranchMoveUpCommand(Ctx));
            bind(_branchMoveUpPopup, () => new BranchMoveUpCommand(Ctx));
            bind(_branchMoveDown, () => new BranchMoveDownCommand(Ctx));
            bind(_branchMoveDownPopup, () => new BranchMoveDownCommand(Ctx));

            bind(_valueEdit, () => new ValueEditStartCommand(Ctx));
            bind(_valueEditPopup, () => new ValueEditStartCommand(Ctx));
            bind(_valueEditMetadata, () => new ValueEditMetadataStartCommand(Ctx));
            bind(_valueEditMetadataPopup, () => new ValueEditMetadataStartCommand(Ctx));
            bind(_valueRename, () => new ValueRenameStartCommand(Ctx));
            bind(_valueRenamePopup, () => new ValueRenameStartCommand(Ctx));
            bind(_valueDelete, () => new ValueDeleteCommand(Ctx));
            bind(_valueDeletePopup, () => new ValueDeleteCommand(Ctx));

            bind(_editCut, () => new CutCommand(Ctx));
            bind(_editCopy, () => new CopyCommand(Ctx));
            bind(_editPaste, () => new PasteCommand(Ctx));
            bind(_branchCutPopup, () => new BranchCutCommand(Ctx));
            bind(_branchCopyPopup, () => new BranchCopyCommand(Ctx));
            bind(_branchPastePopup, () => new BranchPasteCommand(Ctx));
            bind(_valueCutPopup, () => new ValueCutCommand(Ctx));
            bind(_valueCopyPopup, () => new ValueCopyCommand(Ctx));
            bind(_valuePastePopup, () => new ValuePasteCommand(Ctx));
            bind(_valueCutPopup2, () => new ValueCutCommand(Ctx));
            bind(_valueCopyPopup2, () => new ValueCopyCommand(Ctx));
            bind(_valuePastePopup2, () => new ValuePasteCommand(Ctx));

            // Step 2. Generate and bind value creation actions

            Action<ToolStripItemCollection> generateAndBind = coll => 
            {
                var generated = ContentTypes.All.ToDictionary(t => new ToolStripMenuItem(t.LocNewValue) { Tag = t }, t => t);
                generated.ForEach(kvp => bind(kvp.Key, () => new ValueNewTypedCommand(Ctx, kvp.Value.TypeToken)));

                var binary = new ToolStripMenuItem(Resources.New_ValueBinary);
                bind(binary, () => new ValueNewBinaryCommand(Ctx));

                var dummy = coll.Cast<ToolStripItem>().Single(item => item.Name.Contains("Dummy"));
                var index = coll.IndexOf(dummy);

                dummy.Visible = false;
                coll.Insert(index, binary);
                generated.ForEach(kvp => coll.Insert(index, kvp.Key));
            };

            generateAndBind(_value.DropDownItems);
            generateAndBind(_branchPopup.Items);
            generateAndBind(_valuePopup2.Items);

            // Step 3. Generate and bind view actions

            var map = new Dictionary<String, ToolStripMenuItem>();
            VaultViewFactories.All.OrderBy(v => v.LocName).ForEach(v =>
            {
                var nameTemplate = Resources.Views_MenuItem_Template;
                var moduleVersion = v.Type.Assembly.GetName().Version.ToString();
                var miName = String.Format(nameTemplate, v.LocName, moduleVersion);

                var mi = new ToolStripMenuItem(miName);
                map.Add(v.Name, mi);
                _views.DropDownItems.Add(mi);
                bind(mi, () => new ViewToggleCommand(Ctx, v.Name));
            });

            _views.Tag = map; // hack

            // rearrange menu items on view stack changes: push...
            Ctx.Views.ItemPushed += (o, e) =>
            {
                var mi = map[e.Item.Name];
                mi.Checked = true;
                _views.DropDownItems.Remove(mi);
                var iofSep = _views.DropDownItems.IndexOf(_separator13);
                _views.DropDownItems.Insert(iofSep, mi);
            };

            // ...and pop
            Ctx.Views.ItemPopped += (o, e) =>
            {
                var mi = map[e.Item.Name];
                mi.Checked = false;
                _views.DropDownItems.Remove(mi);

                var iofDummy = _views.DropDownItems.IndexOf(_viewsAvailableDummy);
                var lo = iofDummy;
                var hi = _views.DropDownItems.Count;
                Func<int, String> text = i =>
                    i == iofDummy ? "\x0000" :
                    i == _views.DropDownItems.Count ? "\xffff" :
                    _views.DropDownItems[i].Text;

                while (hi - lo > 1)
                {
                    var mid = (int)Math.Ceiling((lo + hi) / 2.0);
                    var cmp = String.CompareOrdinal(text(mid), mi.Text);
                    (cmp != 0).AssertTrue();
                    if (cmp == -1) lo = mid;
                    if (cmp == 1) hi = mid;
                }

                // insert in the exact position to remain sorted
                _views.DropDownItems.Insert(lo + 1, mi);
            };

            // update visibility of menu parts
            _commands.AfterUIResync += (o, e) =>
            {
                var active = Ctx.Views.ToArray();
                var available = VaultViewFactories.All.Where(v_av => 
                    !active.Any(v_ac => v_ac.Name == v_av.Name)).ToArray();

                _views.Visible = active.IsNotEmpty() || available.IsNotEmpty();
                _viewsActive.Visible = active.IsNotEmpty();
                _separator13.Visible = active.IsNotEmpty() && available.IsNotEmpty();
                _viewsAvailable.Visible = available.IsNotEmpty();

                var realMenuItems = _views.DropDownItems.OfType<ToolStripMenuItem>()
                    .Except(new[] {_viewsActive, _viewsAvailable})
                    .Where(mi => !mi.Name.ToLower().Contains("dummy"));
                _views.Enabled = realMenuItems.Any(mi => mi.Enabled);
            };

            // Step 4. Ensure event sources are always synced with the commands

            _commands.ResyncUI();
            _tree.AfterSelect += (o, e) => _commands.ResyncUI();
            _tree.NodeMouseClick += (o, e) => { _tree.SelectedNode = e.Node; _commands.ResyncUI(); };
            _list.SelectedIndexChanged += (o, e) => _commands.ResyncUI();
            _tree.LostFocus += (o, e) => _commands.ResyncUI();
            _tree.GotFocus += (o, e) => _commands.ResyncUI();
            _list.LostFocus += (o, e) => _commands.ResyncUI();
            _list.GotFocus += (o, e) => _commands.ResyncUI();

            // Step 5. Ensure that only one command can be executed at a time

            Ctx.CommandExecuting += (o, e) => { Ctx.CommandsAllowed = false; _commands.ResyncUI(); };
            Ctx.CommandUnexecuting += (o, e) => { Ctx.CommandsAllowed = false; _commands.ResyncUI(); };
            Ctx.CommandExecutionCancelled += (o, e) => { Ctx.CommandsAllowed = true; _commands.ResyncUI(); };
            Ctx.CommandUnexecutionCancelled += (o, e) => { Ctx.CommandsAllowed = true; _commands.ResyncUI(); };
            Ctx.CommandExecuted += (o, e) => { Ctx.CommandsAllowed = true; _commands.ResyncUI(); };
            Ctx.CommandUnexecuted += (o, e) => { Ctx.CommandsAllowed = true; _commands.ResyncUI(); };

            // Step 6. Setup UI update feedback

            _tree.AfterSelect += (o, e) => Ctx.RebuildListItems();
            Ctx.CommandExecuted += (o, e) => UpdateTitle();
            _commands.BeforeUIResync += (o, e) => { if (BeforeUIResync != null) BeforeUIResync(o, e); };
            _commands.AfterUIResync += (o, e) => { if (AfterUIResync != null) AfterUIResync(o, e); };

            // Step 7. Load UI extensions
            _extensions = DataVaultUIExtensions.All.ToArray();
            _extensions.ForEach(ext => ext.Initialize(Ctx));

            // 8. Now once we're all started, we can proceed to loading vault from initial URI
            LoadFromInitialUri();
        }

        private void LoadFromInitialUri()
        {
            var uri = Ctx.InitialUri;
            if (uri.IsNeitherNullNorEmpty())
            {
                var fmt = VaultFormats.Infer(uri);
                if (fmt != null)
                {
                    var vault = fmt.OpenCore(uri);
                    if (vault != null)
                    {
                        Ctx.SetVault(vault, true);
                        Ctx.Execute(new CtxInitializeViews(Ctx));
                    }
                }
                else
                {
                    var title = Resources.ShellIntegration_UnknownFormat_Title;
                    var message = Resources.ShellIntegration_UnknownFormat_Message;
                    message = String.Format(message, uri, VaultFormats.All.Select(fmt1 => fmt1.Name).StringJoin());
                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CleanupOnce()
        {
            // Step 1. Unload extensions
            _extensions.ForEach(ext => ext.Uninitialize());

            // Step 2. Dispose the context (this will possibly dispose views and the vault)
            if (Ctx != null)
            {
                Ctx.Dispose();
            }
        }

        internal void UpdateTitle()
        {
            var fname = Ctx.Vault == null ? Resources.MainForm_Title_NA : Ctx.Vault.Uri;
            var moduleVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var structureVersion = StructureVersion.Current;

            var title = String.Format(Resources.MainForm_Title, moduleVersion, structureVersion, fname);
            Text = String.Format(title + "{0}", Ctx.IsDirty ? " *" : String.Empty);
        }

        private void _tree_BeforeLabelEdit(Object sender, NodeLabelEditEventArgs e)
        {
            e.CancelEdit = !UseControlShortcuts || !new BranchRenameStartCommand(Ctx).CanDoImpl();
        }

        private void _tree_AfterLabelEdit(Object sender, NodeLabelEditEventArgs e)
        {
            var cmd = new BranchRenameFinishCommand(Ctx, e.Label ?? e.Node.Text);
            if (cmd.CanDoImpl()) e.CancelEdit = !Ctx.Execute(cmd);
        }

        private void _list_BeforeLabelEdit(Object sender, LabelEditEventArgs e)
        {
            e.CancelEdit = !UseControlShortcuts || !new ValueRenameStartCommand(Ctx).CanDoImpl();
        }

        private void _list_AfterLabelEdit(Object sender, LabelEditEventArgs e)
        {
            var cmd = new ValueRenameFinishCommand(Ctx, e.Label ?? _list.Items[e.Item].Text);
            if (cmd.CanDoImpl()) e.CancelEdit = !Ctx.Execute(cmd);
        }

        private void _tree_KeyDown(Object sender, KeyEventArgs e)
        {
            if (UseControlShortcuts && e.KeyCode == Keys.F2 && !e.Control && !e.Alt && !e.Shift)
            {
                if (_tree.SelectedNode != null)
                    _tree.SelectedNode.BeginEdit();
            }

            if (UseControlShortcuts && e.KeyCode == Keys.Delete && !e.Control && !e.Alt && !e.Shift)
            {
                var delete = new BranchDeleteCommand(Ctx);
                if (delete.CanDoImpl()) Ctx.Execute(delete);
            }

            if (UseControlShortcuts && e.KeyCode == Keys.Apps && !e.Control && !e.Alt && !e.Shift)
            {
                if (_tree.SelectedNode != null)
                {
                    var vpos = _tree.SelectedNode.Bounds.Location;
                    _tree_MouseClick(this, new MouseEventArgs(MouseButtons.Right, 1, vpos.X, vpos.Y, 0));
                }
            }

            if (UseControlShortcuts && !UseMenuShortcuts)
            {
                ICommand cmd = null;

                if (e.KeyCode == Keys.C && e.Control && !e.Alt && !e.Shift) cmd = new BranchCopyCommand(Ctx);
                if (e.KeyCode == Keys.X && e.Control && !e.Alt && !e.Shift) cmd = new BranchCutCommand(Ctx);
                if (e.KeyCode == Keys.V && e.Control && !e.Alt && !e.Shift) cmd = new BranchPasteCommand(Ctx);
                if (e.KeyCode == Keys.Up && e.Control && !e.Alt && !e.Shift) cmd = new BranchMoveUpCommand(Ctx);
                if (e.KeyCode == Keys.Down && e.Control && !e.Alt && !e.Shift) cmd = new BranchMoveDownCommand(Ctx);
                if (e.KeyCode == Keys.Y && e.Control && !e.Alt && !e.Shift) cmd = new RedoCommand(Ctx);
                if (e.KeyCode == Keys.Z && e.Control && !e.Alt && !e.Shift) cmd = new UndoCommand(Ctx);

                if (e.KeyCode == Keys.N && e.Control && !e.Alt && !e.Shift) cmd = new VaultNewCommand(Ctx);
                if (e.KeyCode == Keys.O && e.Control && !e.Alt && !e.Shift) cmd = new VaultOpenCommand(Ctx);
                if (e.KeyCode == Keys.S && e.Control && !e.Alt && !e.Shift) cmd = new VaultSaveCommand(Ctx);
                if (e.KeyCode == Keys.I && e.Control && !e.Alt && !e.Shift) cmd = new VaultImportCommand(Ctx);
                if (e.KeyCode == Keys.E && e.Control && !e.Alt && !e.Shift) cmd = new VaultExportCommand(Ctx);

                if (cmd != null)
                {
                    if (cmd.CanDo()) Ctx.Execute(cmd);
                }
            }
        }

        private void _list_KeyDown(Object sender, KeyEventArgs e)
        {
            if (UseControlShortcuts && e.KeyCode == Keys.F2 && !e.Control && !e.Alt && !e.Shift)
            {
                if (_list.SelectedItems.Count > 0)
                    _list.SelectedItems[0].BeginEdit();
            }

            if (UseControlShortcuts && e.KeyCode == Keys.Delete && !e.Control && !e.Alt && !e.Shift)
            {
                var delete = new ValueDeleteCommand(Ctx);
                if (delete.CanDoImpl()) Ctx.Execute(delete);
            }

            if (UseControlShortcuts && e.KeyCode == Keys.Apps && !e.Control && !e.Alt && !e.Shift)
            {
                if (_list.SelectedItems.Count > 0)
                {
                    var vpos = _list.SelectedItems[0].Bounds.Location;
                    _list_MouseUp(this, new MouseEventArgs(MouseButtons.Right, 1, vpos.X, vpos.Y, 0));
                }
                else
                {
                    _list_MouseUp(this, new MouseEventArgs(MouseButtons.Right, 1, -1, -1, 0));
                }
            }

            if (UseControlShortcuts && e.KeyData == Keys.Enter && !e.Control && !e.Alt && !e.Shift)
            {
                if (_list.SelectedItems.Count > 0)
                {
                    var vpos = _list.SelectedItems[0].Bounds.Location;
                    _list_MouseDoubleClick(this, new MouseEventArgs(MouseButtons.Left, 2, vpos.X, vpos.Y, 0));
                }
            }

            if (UseControlShortcuts && !UseMenuShortcuts)
            {
                ICommand cmd = null;

                if (e.KeyCode == Keys.C && e.Control && !e.Alt && !e.Shift) cmd = new ValueCopyCommand(Ctx);
                if (e.KeyCode == Keys.X && e.Control && !e.Alt && !e.Shift) cmd = new ValueCutCommand(Ctx);
                if (e.KeyCode == Keys.V && e.Control && !e.Alt && !e.Shift) cmd = new ValuePasteCommand(Ctx);
                if (e.KeyCode == Keys.Y && e.Control && !e.Alt && !e.Shift) cmd = new RedoCommand(Ctx);
                if (e.KeyCode == Keys.Z && e.Control && !e.Alt && !e.Shift) cmd = new UndoCommand(Ctx);

                if (e.KeyCode == Keys.N && e.Control && !e.Alt && !e.Shift) cmd = new VaultNewCommand(Ctx);
                if (e.KeyCode == Keys.O && e.Control && !e.Alt && !e.Shift) cmd = new VaultOpenCommand(Ctx);
                if (e.KeyCode == Keys.S && e.Control && !e.Alt && !e.Shift) cmd = new VaultSaveCommand(Ctx);
                if (e.KeyCode == Keys.I && e.Control && !e.Alt && !e.Shift) cmd = new VaultImportCommand(Ctx);
                if (e.KeyCode == Keys.E && e.Control && !e.Alt && !e.Shift) cmd = new VaultExportCommand(Ctx);

                if (cmd != null)
                {
                    if (cmd.CanDo()) Ctx.Execute(cmd);
                }
            }
        }

        private void _list_MouseUp(Object sender, MouseEventArgs e)
        {
            if (e.Clicks == 1 && e.Button == MouseButtons.Right)
            {
                if (_list.GetItemAt(e.X, e.Y) != null)
                {
                    if (ShowValuePopup && _valuePopup.Enabled)
                    {
                        _valuePopup.Show(_list.PointToScreen(e.Location));
                    }
                }
                else
                {
                    if (ShowValuePopup && _valuePopup2.Enabled)
                    {
                        _valuePopup2.Show(_list.PointToScreen(e.Location));
                    }
                }
            }
        }

        private void _list_MouseClick(Object sender, MouseEventArgs e)
        {
            // code moved up to MouseUp handler to fix the "no event when clicking on empty space" problem
        }

        private void _tree_MouseClick(Object sender, MouseEventArgs e)
        {
            if (e.Clicks == 1 && e.Button == MouseButtons.Right)
            {
                if (_tree.GetNodeAt(e.X, e.Y) != null && _branchPopup.Enabled)
                {
                    if (ShowBranchPopup && _branchPopup.Enabled)
                    {
                        _branchPopup.Show(_tree.PointToScreen(e.Location));
                    }
                }
            }
        }

        private void _list_MouseDoubleClick(Object sender, MouseEventArgs e)
        {
            if (UseControlShortcuts && e.Clicks == 2 && e.Button == MouseButtons.Left)
            {
                var value = _list.SelectedItems.Count == 0 ? null : (IValue)_list.SelectedItems[0].Tag;
                if (value != null)
                {
                    if (value.GetTypeToken2() == "binary")
                    {
                        if (MessageBox.Show(Resources.BinaryType_ConfirmExtractionToView, Resources.Confirmation_Title,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                        {
                            var temp = Path.GetTempPath();
                            var subdir = new DirectoryInfo(temp).CreateSubdirectory(Guid.NewGuid().ToString());
                            var fullName = subdir + @"\" + value.Name;
                            File.WriteAllBytes(fullName, value.ContentStream.AsByteArray());

                            try
                            {
                                Process.Start(fullName);
                            }
                            catch(Win32Exception w32ex)
                            {
                                if (w32ex.NativeErrorCode == 1155)
                                {
                                    try
                                    {
                                        Process.Start("explorer.exe", subdir.FullName);
                                    }
                                    catch
                                    {
                                        MessageBox.Show(Resources.BinaryType_ErrorOpeningContentFile,
                                            Resources.Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(Resources.BinaryType_ErrorOpeningContentFile,
                                        Resources.Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                }
                            }
                            catch
                            {
                                MessageBox.Show(Resources.BinaryType_ErrorOpeningContentFile,
                                    Resources.Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                        }
                    }
                    else
                    {
                        var cmd = new ValueEditStartCommand(Ctx);
                        if (cmd.CanDoImpl()) Ctx.Execute(cmd);
                    }
                }
            }
        }

        private void _tree_ItemDrag(Object sender, ItemDragEventArgs e)
        {
            var item = (TreeNode)e.Item;
            if (UseControlShortcuts && item.Parent != null)
                DoDragDrop(item, DragDropEffects.Move);
        }

        private void _tree_DragEnter(Object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void _tree_DragOver(Object sender, DragEventArgs e)
        {
            var targetPoint = _tree.PointToClient(new Point(e.X, e.Y));
            _tree.SelectedNode = _tree.GetNodeAt(targetPoint);
        }

        private void _tree_DragDrop(Object sender, DragEventArgs e)
        {
            var draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
            var cmd = new BranchDragDropCommand(Ctx, (IBranch)draggedNode.Tag);
            if (cmd.CanDoImpl()) Ctx.Execute(cmd);
        }
    }
}
