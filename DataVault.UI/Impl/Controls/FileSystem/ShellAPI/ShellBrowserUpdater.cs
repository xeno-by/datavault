using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DataVault.UI.Impl.Controls.FileSystem.ShellAPI
{
    internal class ShellBrowserUpdater : NativeWindow
    {
        #region Fields

        private ShellBrowserComponent br;
        private uint notifyId = 0;

        #endregion

        public ShellBrowserUpdater(ShellBrowserComponent br)
        {
            this.br = br;
            CreateHandle(new CreateParams());

            NativeShellAPI.SHChangeNotifyEntry entry = new NativeShellAPI.SHChangeNotifyEntry();
            entry.pIdl = br.DesktopItem.PIDLRel.Ptr;
            entry.Recursively = true;

            notifyId = NativeShellAPI.SHChangeNotifyRegister(
                this.Handle,
                NativeShellAPI.SHCNRF.InterruptLevel | NativeShellAPI.SHCNRF.ShellLevel,
                NativeShellAPI.SHCNE.ALLEVENTS | NativeShellAPI.SHCNE.INTERRUPT,
                NativeShellAPI.WM.SH_NOTIFY,
                1,
                new NativeShellAPI.SHChangeNotifyEntry[] { entry });
        }

        ~ShellBrowserUpdater()
        {
            if (notifyId > 0)
            {
                NativeShellAPI.SHChangeNotifyDeregister(notifyId);
                GC.SuppressFinalize(this);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)NativeShellAPI.WM.SH_NOTIFY)
            {
                NativeShellAPI.SHNOTIFYSTRUCT shNotify =
                    (NativeShellAPI.SHNOTIFYSTRUCT)Marshal.PtrToStructure(m.WParam, typeof(NativeShellAPI.SHNOTIFYSTRUCT));

                //Console.Out.WriteLine("Event: {0}", (ShellAPI.SHCNE)m.LParam);
                //if (shNotify.dwItem1 != IntPtr.Zero)
                    //PIDL.Write(shNotify.dwItem1);
                //if (shNotify.dwItem2 != IntPtr.Zero)
                    //PIDL.Write(shNotify.dwItem2);

                switch ((NativeShellAPI.SHCNE)m.LParam)
                {
                    #region File Changes

                    case NativeShellAPI.SHCNE.CREATE:
                        #region Create Item
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1))
                            {
                                IntPtr parent, child, relative;
                                PIDL.SplitPidl(shNotify.dwItem1, out parent, out child);

                                PIDL parentPIDL = new PIDL(parent, false);
                                ShellItem parentItem = br.GetShellItem(parentPIDL);
                                if (parentItem != null && parentItem.FilesExpanded && !parentItem.SubFiles.Contains(child))
                                {
                                    NativeShellAPI.SHGetRealIDL(
                                        parentItem.ShellFolder,
                                        child,
                                        out relative);
                                    parentItem.AddItem(new ShellItem(br, parentItem, relative));
                                }
                                    
                                Marshal.FreeCoTaskMem(child);
                                parentPIDL.Free();
                            }
                        }
                        #endregion
                        break;

                    case NativeShellAPI.SHCNE.RENAMEITEM:
                        #region Rename Item
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1) && !PIDL.IsEmpty(shNotify.dwItem2))
                            {
                                ShellItem item = br.GetShellItem(new PIDL(shNotify.dwItem1, true));
                                if (item != null)
                                    item.Update(shNotify.dwItem2, ShellItemUpdateType.Renamed);
                            }
                        }
                        #endregion
                        break;

                    case NativeShellAPI.SHCNE.DELETE:
                        #region Delete Item
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1))
                            {
                                IntPtr parent, child;
                                PIDL.SplitPidl(shNotify.dwItem1, out parent, out child);

                                PIDL parentPIDL = new PIDL(parent, false);
                                ShellItem parentItem = br.GetShellItem(parentPIDL);
                                if (parentItem != null && parentItem.SubFiles.Contains(child))
                                    parentItem.RemoveItem(parentItem.SubFiles[child]);

                                Marshal.FreeCoTaskMem(child);
                                parentPIDL.Free();
                            }
                        }
                        #endregion
                        break;

                    case NativeShellAPI.SHCNE.UPDATEITEM:
                        #region Update Item
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1))
                            {
                                ShellItem item = br.GetShellItem(new PIDL(shNotify.dwItem1, true));
                                if (item != null)
                                {
                                    Console.Out.WriteLine("Item: {0}", item);
                                    item.Update(IntPtr.Zero, ShellItemUpdateType.Updated);
                                    item.Update(IntPtr.Zero, ShellItemUpdateType.IconChange);
                                }
                            }
                        }
                        #endregion
                        break;

                    #endregion

                    #region Folder Changes

                    case NativeShellAPI.SHCNE.MKDIR:                        
                    case NativeShellAPI.SHCNE.DRIVEADD:
                    case NativeShellAPI.SHCNE.DRIVEADDGUI:                        
                        #region Make Directory
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1))
                            {
                                IntPtr parent, child, relative;
                                PIDL.SplitPidl(shNotify.dwItem1, out parent, out child);

                                PIDL parentPIDL = new PIDL(parent, false);
                                ShellItem parentItem = br.GetShellItem(parentPIDL);
                                if (parentItem != null && parentItem.FoldersExpanded && !parentItem.SubFolders.Contains(child))
                                {
                                    NativeShellAPI.SHGetRealIDL(
                                        parentItem.ShellFolder,
                                        child,
                                        out relative);

                                    IntPtr shellFolderPtr;
                                    if (parentItem.ShellFolder.BindToObject(
                                                relative,
                                                IntPtr.Zero,
                                                ref NativeShellAPI.IID_IShellFolder,
                                                out shellFolderPtr) == NativeShellAPI.S_OK)
                                    {
                                        parentItem.AddItem(new ShellItem(br, parentItem, relative, shellFolderPtr));
                                    }
                                    else
                                        Marshal.FreeCoTaskMem(relative);
                                }

                                Marshal.FreeCoTaskMem(child);
                                parentPIDL.Free();
                            }
                        }
                        #endregion
                        break;

                    case NativeShellAPI.SHCNE.RENAMEFOLDER:
                        #region Rename Directory
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1) && !PIDL.IsEmpty(shNotify.dwItem2))
                            {
                                ShellItem item = br.GetShellItem(new PIDL(shNotify.dwItem1, false));
                                if (item != null)
                                {
                                    //Console.Out.WriteLine("Update: {0}", item);
                                    item.Update(shNotify.dwItem2, ShellItemUpdateType.Renamed);
                                }
                            }
                        }
                        #endregion
                        break;

                    case NativeShellAPI.SHCNE.RMDIR:
                    case NativeShellAPI.SHCNE.DRIVEREMOVED:
                        #region Remove Directory
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1))
                            {
                                IntPtr parent, child;
                                PIDL.SplitPidl(shNotify.dwItem1, out parent, out child);

                                PIDL parentPIDL = new PIDL(parent, false);
                                ShellItem parentItem = br.GetShellItem(parentPIDL);
                                if (parentItem != null && parentItem.SubFolders.Contains(child))
                                    parentItem.RemoveItem(parentItem.SubFolders[child]);

                                Marshal.FreeCoTaskMem(child);
                                parentPIDL.Free();
                            }
                        }
                        #endregion
                        break;

                    case NativeShellAPI.SHCNE.UPDATEDIR:
                    case NativeShellAPI.SHCNE.ATTRIBUTES:
                        #region Update Directory
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1))
                            {
                                ShellItem item = br.GetShellItem(new PIDL(shNotify.dwItem1, true));
                                if (item != null)
                                {
                                    item.Update(IntPtr.Zero, ShellItemUpdateType.Updated);
                                    item.Update(IntPtr.Zero, ShellItemUpdateType.IconChange);
                                }
                            }
                        }
                        #endregion
                        break;

                    case NativeShellAPI.SHCNE.MEDIAINSERTED:
                    case NativeShellAPI.SHCNE.MEDIAREMOVED:
                        #region Media Change
                        {
                            if (!PIDL.IsEmpty(shNotify.dwItem1))
                            {
                                ShellItem item = br.GetShellItem(new PIDL(shNotify.dwItem1, true));
                                if (item != null)
                                    item.Update(IntPtr.Zero, ShellItemUpdateType.MediaChange);
                            }
                        }
                        #endregion
                        break;

                    #endregion

                    #region Other Changes

                    case NativeShellAPI.SHCNE.ASSOCCHANGED:
                        #region Update Images
                        {
                            
                        }
                        #endregion
                        break;

                    case NativeShellAPI.SHCNE.NETSHARE:
                    case NativeShellAPI.SHCNE.NETUNSHARE:
                        break;

                    case NativeShellAPI.SHCNE.UPDATEIMAGE:
                        UpdateRecycleBin();
                        break;

                    #endregion
                }
            }

            base.WndProc(ref m);
        }

        private void UpdateRecycleBin()
        {
            ShellItem recycleBin = br.DesktopItem["Recycle Bin"];
            if (recycleBin != null)
                recycleBin.Update(IntPtr.Zero, ShellItemUpdateType.IconChange);
        }
    }
}
