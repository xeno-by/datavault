using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;

namespace DataVault.UI.Impl.Controls.FileSystem.ShellAPI
{
    internal class ShellBrowserComponent : Component
    {
        #region Fields

        private ShellItem desktopItem;
        private string mydocsName, mycompName, sysfolderName, mydocsPath;

        private ShellBrowserUpdater updater;

        private ArrayList browsers;
        private ShellItemUpdateCondition updateCondition;

        internal event ShellItemUpdateEventHandler ShellItemUpdate;

        #endregion

        public ShellBrowserComponent()
        {
            InitVars();
            browsers = new ArrayList();
            updateCondition = new ShellItemUpdateCondition();
            updater = new ShellBrowserUpdater(this);
        }

        private void InitVars()
        {
            IntPtr tempPidl;
            NativeShellAPI.SHFILEINFO info;

            //My Computer
            info = new NativeShellAPI.SHFILEINFO();
            tempPidl = IntPtr.Zero;
            NativeShellAPI.SHGetSpecialFolderLocation(IntPtr.Zero, NativeShellAPI.CSIDL.DRIVES, out tempPidl);

            NativeShellAPI.SHGetFileInfo(tempPidl, 0, ref info, NativeShellAPI.cbFileInfo,
                NativeShellAPI.SHGFI.PIDL | NativeShellAPI.SHGFI.DISPLAYNAME | NativeShellAPI.SHGFI.TYPENAME);

            sysfolderName = info.szTypeName;
            mycompName = info.szDisplayName;
            Marshal.FreeCoTaskMem(tempPidl);
            //

            //Dekstop
            tempPidl = IntPtr.Zero;
            NativeShellAPI.SHGetSpecialFolderLocation(IntPtr.Zero, NativeShellAPI.CSIDL.DESKTOP, out tempPidl);
            IntPtr desktopFolderPtr;
            NativeShellAPI.SHGetDesktopFolder(out desktopFolderPtr);
            desktopItem = new ShellItem(this, tempPidl, desktopFolderPtr);
            //

            //My Documents
            uint pchEaten = 0;
            NativeShellAPI.SFGAO pdwAttributes = 0;
            desktopItem.ShellFolder.ParseDisplayName(
                IntPtr.Zero,
                IntPtr.Zero,
                "::{450d8fba-ad25-11d0-98a8-0800361b1103}",
                ref pchEaten,
                out tempPidl,
                ref pdwAttributes);

            info = new NativeShellAPI.SHFILEINFO();
            NativeShellAPI.SHGetFileInfo(tempPidl, 0, ref info, NativeShellAPI.cbFileInfo,
                NativeShellAPI.SHGFI.PIDL | NativeShellAPI.SHGFI.DISPLAYNAME);

            mydocsName = info.szDisplayName;
            Marshal.FreeCoTaskMem(tempPidl);

            StringBuilder path = new StringBuilder(NativeShellAPI.MAX_PATH);
            NativeShellAPI.SHGetFolderPath(
                    IntPtr.Zero, NativeShellAPI.CSIDL.PERSONAL,
                    IntPtr.Zero, NativeShellAPI.SHGFP.TYPE_CURRENT, path);
            mydocsPath = path.ToString();
            //
        }

        #region ShellBrowser Update

        internal void OnShellItemUpdate(object sender, ShellItemUpdateEventArgs e)
        {
            if (ShellItemUpdate != null)
            {
                ShellItemUpdate(sender, e);
            }
        }

        #endregion

        #region Utility Methods

        internal ShellItem GetShellItem(PIDL pidlFull)
        {
            ShellItem current = DesktopItem;
            if (pidlFull.Ptr == IntPtr.Zero)
                return current;

            foreach (IntPtr pidlRel in pidlFull)
            {
                int index;
                if ((index = current.IndexOf(pidlRel)) > -1)
                {
                    current = current[index];
                }
                else
                {
                    current = null;
                    break;
                }
            }

            return current;
        }

        internal ShellItem[] GetPath(ShellItem item)
        {
            ArrayList pathList = new ArrayList();
            
            ShellItem currentItem = item;
            while (currentItem.ParentItem != null)
            {
                pathList.Add(currentItem);
                currentItem = currentItem.ParentItem;
            }
            pathList.Add(currentItem);
            pathList.Reverse();

            return (ShellItem[])pathList.ToArray(typeof(ShellItem));
        }

        #endregion

        #region Properties

        internal ShellItem DesktopItem { get { return desktopItem; } }

        internal string MyDocumentsName { get { return mydocsName; } }
        internal string MyComputerName { get { return mycompName; } }
        internal string SystemFolderName { get { return sysfolderName; } }

        internal string MyDocumentsPath { get { return mydocsPath; } }

        internal ShellItemUpdateCondition UpdateCondition { get { return updateCondition; } }

        internal ArrayList Browsers { get { return browsers; } }

        #endregion
    }

    #region ShellItemUpdate

    internal delegate void ShellItemUpdateEventHandler(object sender, ShellItemUpdateEventArgs e);

    internal enum ShellItemUpdateType
    {
        Created,
        IconChange,
        Updated,
        Renamed,
        Deleted,
        MediaChange
    }

    internal class ShellItemUpdateEventArgs : EventArgs
    {
        ShellItem oldItem, newItem;
        ShellItemUpdateType type;

        public ShellItemUpdateEventArgs(
            ShellItem oldItem,
            ShellItem newItem,
            ShellItemUpdateType type)
        {
            this.oldItem = oldItem;
            this.newItem = newItem;
            this.type = type;
        }

        public ShellItem OldItem { get { return oldItem; } }
        public ShellItem NewItem { get { return newItem; } }
        public ShellItemUpdateType UpdateType { get { return type; } }
    }

    #endregion
}
