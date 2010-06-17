using System;
using System.IO;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.VaultFormatz.Dialogs;
using DataVault.UI.Api.Versioning;
using DataVault.UI.Impl.Controls.FileSystem;
using DataVault.UI.Properties;
using DataVault.UI.Api.VaultFormatz;
using DataVault.Core.Helpers;

namespace DataVault.UI.Impl.VaultFormatz
{
    [VaultFormat("fs"), VaultFormatLoc(typeof(Resources), "VaultFormat_Fs_DialogTitle", "VaultFormat_Fs_TabTitle")]
    public class FsVaultFormat : VaultFormat
    {
        public override bool AcceptCore(string uri)
        {
            return Directory.Exists(uri);
        }

        public override IVault OpenCore(String uri)
        {
            return OpenCore(uri, false);
        }

        private IVault OpenCore(String uri, bool forSave)
        {
            try
            {
                if (forSave || (!IndexExists(uri) && new DirectoryInfo(uri).GetFileSystemInfos().IsNullOrEmpty()))
                {
                    if (IndexExists(uri)) File.Delete(Path.Combine(uri, "#index"));
                    File.WriteAllText(Path.Combine(uri, "#index"), String.Empty);
                }

                return StructureVersion.OpenAsLatestStructureVersion(() => VaultApi.OpenFs(uri));
            }
            catch (Exception)
            {
                MessageBox.Show(Resources.Validation_InvalidFsVault, Resources.Validation_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private bool IndexExists(String uri)
        {
            return File.Exists(Path.Combine(uri, "#index"));
        }

        private bool ApproveSavingToUri(String uri)
        {
            if (Directory.Exists(uri))
            {
                if (IndexExists(uri))
                {
                    if (MessageBox.Show(Resources.VaultFormat_ExistingFsVault_Message, Resources.VaultFormat_ExistingFsVault_Title,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        return false;
                    }
                    else
                    {
                        File.Delete(Path.Combine(uri, "#index"));
                    }
                }
                else
                {
                    if (!new DirectoryInfo(uri).GetFileSystemInfos().IsNullOrEmpty())
                    {
                        if (MessageBox.Show(Resources.VaultFormat_NonEmptyFsUri_Message, Resources.VaultFormat_NonEmptyFsUri_Title,
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool ApproveOpeningFromUri(String uri)
        {
            if (Directory.Exists(uri))
            {
                if (IndexExists(uri))
                {
                    return true;
                }
                else
                {
                    return MessageBox.Show(Resources.VaultFormat_NoIndexFileFsUri_Message, Resources.VaultFormat_NoIndexFileFsUr_Title,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
                }
            }
            else
            {
                MessageBox.Show(Resources.Dialogs_BrowseForFolder_FolderDoesNotExist_Message, Resources.Dialogs_BrowseForFolder_FolderDoesNotExist_Title,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public override void BuildDialogControls(IVaultDialog dialog, IVaultDialogTab tab)
        {
            // File dialog emulation

            var shellBrowser = new ShellBrowser();
            shellBrowser.Dock = DockStyle.Fill;
            shellBrowser.ListViewMode = View.List;
            shellBrowser.Multiselect = false;
            shellBrowser.ShowFolders = true;
            shellBrowser.ShowFoldersButton = false;
            shellBrowser.browseSplitter.Panel2Collapsed = true; // hack!

            var lbFolderName = new Label{Text = Resources.Dialogs_BrowseForFolder_FolderNameLabel, Padding = new Padding(2, 7, 5, 3)};
            var tbFolderName = new TextBox(){Dock = DockStyle.Fill};
            var pn1 = new Panel(){Dock = DockStyle.Left};
            pn1.Controls.Add(lbFolderName);
            pn1.Width = 70;
            var pn2 = new Panel(){Dock = DockStyle.Fill, Padding = new Padding(5, 5, 2, 5)};
            pn2.Controls.Add(tbFolderName);

            var pnFolderName = new Panel(){Dock = DockStyle.Bottom};
            pnFolderName.Controls.Add(pn2);
            pnFolderName.Controls.Add(pn1);
            pnFolderName.Height = tbFolderName.Height + 10;
            var pnShellBrowser = new Panel(){ Dock = DockStyle.Fill};
            pnShellBrowser.Controls.Add(shellBrowser);
            tab.TabPage.Controls.Add(pnShellBrowser);
            tab.TabPage.Controls.Add(pnFolderName);

            shellBrowser.HandleCreated += (o, e) =>
            {
                // loads startup dir from appdomain-wide history
                var startupDir = (String)AppDomain.CurrentDomain.GetData(this.GetType().FullName);
                if (!Directory.Exists(startupDir)) startupDir = null;
                startupDir = startupDir ?? shellBrowser.ShellBrowserComponent.MyDocumentsPath;
                shellBrowser.SelectPath(startupDir, false);

                // remember all changes of current directory
                Action storeCurrentDirInAppDomain = () =>
                {
                    var currentDir = ".".ResolveShellPathAsFileSystemPath(shellBrowser);
                    AppDomain.CurrentDomain.SetData(this.GetType().FullName, currentDir);
                };

                shellBrowser.FileView.SelectedIndexChanged += (o1, e1) => storeCurrentDirInAppDomain();
                shellBrowser.FolderView.AfterSelect += (o1, e1) => storeCurrentDirInAppDomain();
            };

            Action refreshTextBox = () =>
            {
                var shellItem = shellBrowser.CurrentDirectory;
                tbFolderName.Text = shellItem == null ? String.Empty : ".";
            };

            shellBrowser.FileView.SelectedIndexChanged += (o, e) => refreshTextBox();
            shellBrowser.FolderView.AfterSelect += (o, e) => refreshTextBox();

            // Ensuring consistent OK button availability

            Action refreshOkAvailability = () =>
            {
                var shellItem = shellBrowser.CurrentDirectory; // hack
                var folderName = tbFolderName.Text;

                if (folderName == (shellItem == null ? String.Empty : shellItem.Text) ||
                    folderName == (shellItem == null ? String.Empty : shellItem.Path))
                {
                    // foldername in the text box corresponds to selected file/dir
                    dialog.OkButton.Enabled = shellItem != null && shellItem.IsFileSystem && shellItem.IsFolder && !shellItem.IsLink;
                }
                else
                {
                    dialog.OkButton.Enabled = folderName.IsNeitherNullNorEmpty();
                }
            };

            dialog.TabActivated += (o, e) => refreshOkAvailability();
            tbFolderName.TextChanged += (o, e) => refreshOkAvailability();

            // Processing folder selection: via OK, via ENTER, or via double-click on a folder in the tree

            Action<String> processSelectFolder = path =>
            {
                var resolved = path.ResolveShellPathAsFileSystemPath(shellBrowser);
                if (resolved == null)
                {
                    MessageBox.Show(
                        Resources.Dialogs_BrowseForFolder_NotAFileSystemLocation_Message,
                        Resources.Dialogs_BrowseForFolder_NotAFileSystemLocation_Title,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    // the logic below only accepts full paths
                    Path.IsPathRooted(resolved).AssertTrue();

                    if (File.Exists(resolved))
                    {
                        MessageBox.Show(
                            Resources.Dialogs_BrowseForFolder_FileIsNotValidHere_Message,
                            Resources.Dialogs_BrowseForFolder_FileIsNotValidHere_Title,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    else
                    {
                        // remember the selection
                        AppDomain.CurrentDomain.SetData(this.GetType().FullName, resolved);

                        if (dialog.Action == VaultAction.Create ||
                            dialog.Action == VaultAction.Export)
                        {
                            if (ApproveSavingToUri(resolved))
                            {
                                resolved.EnsureDirectoryExists();
                                var vault = OpenCore(resolved, true);

                                if (vault != null)
                                {
                                    dialog.EndDialog(vault);
                                }
                            }
                        }
                        else if (dialog.Action == VaultAction.Open ||
                            dialog.Action == VaultAction.Import)
                        {
                            if (ApproveOpeningFromUri(resolved))
                            {
                                var vault = OpenCore(resolved, false);
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

            // disabled on purpose:
            // general guideline is having expand/collapse on double click
            // and even Windows' system browse for folder dialog respects this behaviour

//            shellBrowser.folderView.NodeMouseDoubleClick += (o, e) =>
//            {
//                refreshOkAvailability();
//                if (dialog.OkButton.Enabled)
//                {
//                    processSelectShellItem();
//                }
//            };

            dialog.OkButton.Click += (o, e) =>
            {
                if (dialog.ActiveTab == tab)
                {
                    processSelectFolder(tbFolderName.Text);
                }
            };

            tbFolderName.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.Enter && !e.Alt && !e.Shift && !e.Control)
                {
                    processSelectFolder(tbFolderName.Text);
                }
            };
        }

        private class Wrapped<T>
        {
            public T O { get; set; }
            public Wrapped() { O = default(T); }
            public Wrapped(T o) { O = o; }
        }
    }
}