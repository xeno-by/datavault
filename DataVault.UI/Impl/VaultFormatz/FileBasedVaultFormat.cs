using System;
using System.IO;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Versioning;
using DataVault.UI.Api.VaultFormatz;
using DataVault.UI.Api.VaultFormatz.Dialogs;
using DataVault.UI.Impl.Controls.FileSystem;
using DataVault.UI.Impl.Controls.FileSystem.ShellAPI;
using DataVault.UI.Properties;
using System.Linq;

namespace DataVault.UI.Impl.VaultFormatz
{
    public abstract class FileBasedVaultFormat : VaultFormat
    {
        protected abstract bool AcceptVaultCore(String uri);
        protected abstract IVault OpenVaultCore(String uri);
        protected abstract bool VaultExists(String uri);

        public override IVault OpenCore(String uri)
        {
            return StructureVersion.OpenAsLatestStructureVersion(() => OpenVaultCore(uri));
        }

        public override bool AcceptCore(string uri)
        {
            return File.Exists(uri) && AcceptVaultCore(uri);
        }

        public override void BuildDialogControls(IVaultDialog dialog, IVaultDialogTab tab)
        {
            // File dialog emulation

            var shellBrowser = new ShellBrowser();
            shellBrowser.Dock = DockStyle.Fill;
            shellBrowser.ListViewMode = View.List;
            shellBrowser.Multiselect = false;
            shellBrowser.ShowFolders = false;
            shellBrowser.ShowFoldersButton = true;

            var lbFileName = new Label { Text = Resources.Dialogs_BrowseForFile_FileNameLabel, Padding = new Padding(2, 7, 5, 3)};
            var tbFileName = new TextBox(){Dock = DockStyle.Fill};
            var pn1 = new Panel(){Dock = DockStyle.Left};
            pn1.Controls.Add(lbFileName);
            pn1.Width = 70;
            var pn2 = new Panel(){Dock = DockStyle.Fill, Padding = new Padding(5, 5, 2, 5)};
            pn2.Controls.Add(tbFileName);

            var pnFileName = new Panel(){Dock = DockStyle.Bottom};
            pnFileName.Controls.Add(pn2);
            pnFileName.Controls.Add(pn1);
            pnFileName.Height = tbFileName.Height + 10;
            var pnShellBrowser = new Panel(){ Dock = DockStyle.Fill};
            pnShellBrowser.Controls.Add(shellBrowser);
            tab.TabPage.Controls.Add(pnShellBrowser);
            tab.TabPage.Controls.Add(pnFileName);

            Action refreshTextBox = () =>
            {
                var shellItem = shellBrowser.SelectedItem;
                tbFileName.Text = shellItem == null ? String.Empty : shellItem.Text;
            };

            shellBrowser.FileView.SelectedIndexChanged += (o, e) => refreshTextBox();
            shellBrowser.FolderView.AfterSelect += (o, e) => refreshTextBox();

            // Ensuring consistent OK button availability

            Action refreshOkAvailability = () =>
            {
                var shellItem = shellBrowser.SelectedItem;
                var fileName = tbFileName.Text;

                if (fileName == (shellItem == null ? String.Empty : shellItem.Text) ||
                    fileName == (shellItem == null ? String.Empty : shellItem.Path))
                {
                    // filename in the text box corresponds to selected file/dir
                    dialog.OkButton.Enabled = shellItem != null && shellItem.IsFileSystem && !shellItem.IsFolder && !shellItem.IsLink;
                }
                else
                {
                    dialog.OkButton.Enabled = fileName.IsNeitherNullNorEmpty();
                }
            };

            dialog.TabActivated += (o, e) => refreshOkAvailability();
            tbFileName.TextChanged += (o, e) => refreshOkAvailability();

            // Processing file selection: via OK, via ENTER, or via double-click on a file

            Action<String> processSelectFile = fpath =>
            {
                var resolved = fpath.ResolveShellPathAsFileSystemPath(shellBrowser);
                if (resolved == null)
                {
                    MessageBox.Show(
                        Resources.Dialogs_BrowseForFile_NotAFileSystemLocation_Message,
                        Resources.Dialogs_BrowseForFile_NotAFileSystemLocation_Title,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    // the logic below only accepts full paths
                    Path.IsPathRooted(resolved).AssertTrue();

                    if (Directory.Exists(resolved))
                    {
                        MessageBox.Show(
                            Resources.Dialogs_BrowseForFile_FolderIsNotValidHere_Message,
                            Resources.Dialogs_BrowseForFile_FolderIsNotValidHere_Title,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    else
                    {
                        // remember directory-container of the selected file
                        AppDomain.CurrentDomain.SetData(this.GetType().FullName, resolved);

                        if (dialog.Action == VaultAction.Create ||
                            dialog.Action == VaultAction.Export)
                        {
                            if (File.Exists(resolved))
                            {
                                if (MessageBox.Show(
                                        Resources.VaultFormat_ExistingFileBasedVault_Message,
                                        Resources.VaultFormat_ExistingFileBasedVault_Title,
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Exclamation) == DialogResult.Yes)
                                {
                                    File.Delete(resolved);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                var parentDir = Path.GetDirectoryName(resolved);
                                parentDir.EnsureDirectoryExists();
                                // the file itself will be created by vault logic
                            }

                            IVault vault;
                            var dialogForm = tab.TabPage.Parent.Parent.Parent;
                            var oldCursor = dialogForm.Cursor;
                            dialogForm.Cursor = Cursors.WaitCursor;

                            try
                            {
                                vault = OpenCore(resolved);
                            }
                            finally
                            {
                                dialogForm.Cursor = oldCursor;
                            }

                            if (vault != null)
                            {
                                dialog.EndDialog(vault);
                            }
                        }
                        else if (dialog.Action == VaultAction.Open ||
                            dialog.Action == VaultAction.Import)
                        {
                            if (!File.Exists(resolved))
                            {
                                MessageBox.Show(
                                    Resources.Dialogs_BrowseForFile_FileDoesNotExist_Message,
                                    Resources.Dialogs_BrowseForFile_FileDoesNotExist_Title,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                            }
                            else
                            {
                                IVault vault;
                                var dialogForm = tab.TabPage.Parent.Parent.Parent;
                                var oldCursor = dialogForm.Cursor;
                                dialogForm.Cursor = Cursors.WaitCursor;

                                try
                                {
                                    vault = OpenCore(resolved);
                                }
                                finally
                                {
                                    dialogForm.Cursor = oldCursor;
                                }

                                if (vault != null)
                                {
                                    dialog.EndDialog(vault);
                                }
                            }
                        }
                        else
                        {
                            AssertionHelper.Fail();
                        }
                    }
                }
            };

            // ENTER over the filename textbox
            tbFileName.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Alt && !e.Shift && !e.Control)
                {
                    processSelectFile(tbFileName.Text);
                }
            };

            // OK click
            dialog.OkButton.Click += (o, e) =>
            {
                if (dialog.ActiveTab == tab)
                {
                    processSelectFile(tbFileName.Text);
                }
            };

            // double-click or ENTER over the files list
            Action onValidItemActivated = () =>
            {
                var shellItem = shellBrowser.SelectedItem;
                shellItem.AssertNotNull();
                shellItem.IsFileSystem.AssertTrue();
                shellItem.IsFolder.AssertFalse();
                shellItem.IsLink.AssertFalse();

                processSelectFile(shellItem.Text);
            };

            // Stuff that needs to be run on after the handle is created

            shellBrowser.HandleCreated += (o, e) =>
            {
                // loads startup dir from appdomain-wide history
                var prevFile = (String)AppDomain.CurrentDomain.GetData(this.GetType().FullName);
                var startupDir = Path.GetDirectoryName(prevFile);
                if (!Directory.Exists(startupDir)) startupDir = null;
                startupDir = startupDir ?? shellBrowser.ShellBrowserComponent.MyDocumentsPath;
                shellBrowser.SelectPath(startupDir, false);

                // selects previously selected file in the file view...
                var match = shellBrowser.FileView.Items.Cast<ListViewItem>().SingleOrDefaultDontCrash(lvi =>
                {
                    var si = lvi.Tag.AssertCast<ShellItem>();
                    return si.Text.ResolveShellPathAsFileSystemPath(shellBrowser) == prevFile;
                });

                // ...if it still exists
                if (match != null)
                {
                    match.Selected = true;
                }

                // remember all changes of current directory
                Action storeCurrentDirInAppDomain = () =>
                {
                    var currentDir = ".".ResolveShellPathAsFileSystemPath(shellBrowser);
                    AppDomain.CurrentDomain.SetData(this.GetType().FullName, Path.Combine(currentDir, "aux"));
                };

                shellBrowser.FileView.SelectedIndexChanged += (o1, e1) => storeCurrentDirInAppDomain();
                shellBrowser.FolderView.AfterSelect += (o1, e1) => storeCurrentDirInAppDomain();

                // make sure that double-clicking on a file doesn't open associated application
                // but just selects that file instead
                var fv = shellBrowser.fileView;
                var itemActivate = typeof(ListView).GetEvent("ItemActivate");
                var invocationList = itemActivate.GetInvocationList(fv);
                itemActivate.ClearInvocationList(fv);
                fv.ItemActivate += (o1, e1) =>
                {
                    refreshOkAvailability();
                    if (dialog.OkButton.Enabled)
                    {
                        onValidItemActivated();
                    }
                    else
                    {
                        invocationList.ForEach(d => d.DynamicInvoke(o1, e1));
                    }
                };
            };
        }
    }
}