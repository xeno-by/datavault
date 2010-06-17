using System;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DataVault.UI.Impl.Controls.FileSystem.ShellAPI
{
    internal static class ShellImageList
    {
        private static IntPtr smallImageListHandle, largeImageListHandle;
        private static Hashtable imageTable;

        private const int TVSIL_NORMAL = 0;
        private const int TVSIL_SMALL = 1;

        static ShellImageList()
        {
            imageTable = new Hashtable();

            NativeShellAPI.SHGFI flag = NativeShellAPI.SHGFI.USEFILEATTRIBUTES | NativeShellAPI.SHGFI.SYSICONINDEX | NativeShellAPI.SHGFI.SMALLICON;
            NativeShellAPI.SHFILEINFO shfiSmall = new NativeShellAPI.SHFILEINFO();
            smallImageListHandle = NativeShellAPI.SHGetFileInfo(".txt", NativeShellAPI.FILE_ATTRIBUTE.NORMAL, ref shfiSmall, NativeShellAPI.cbFileInfo, flag);

            flag = NativeShellAPI.SHGFI.USEFILEATTRIBUTES | NativeShellAPI.SHGFI.SYSICONINDEX | NativeShellAPI.SHGFI.LARGEICON;
            NativeShellAPI.SHFILEINFO shfiLarge = new NativeShellAPI.SHFILEINFO();
            largeImageListHandle = NativeShellAPI.SHGetFileInfo(".txt", NativeShellAPI.FILE_ATTRIBUTE.NORMAL, ref shfiLarge, NativeShellAPI.cbFileInfo, flag);
        }

        internal static void SetIconIndex(ShellItem item, int index, bool SelectedIcon)
        {
            bool HasOverlay = false; //true if it's an overlay
            int rVal = 0; //The returned Index

            NativeShellAPI.SHGFI dwflag = NativeShellAPI.SHGFI.SYSICONINDEX | NativeShellAPI.SHGFI.PIDL | NativeShellAPI.SHGFI.ICON;
            NativeShellAPI.FILE_ATTRIBUTE dwAttr = 0;
            //build Key into HashTable for this Item
            int Key = index * 256;
            if (item.IsLink)
            {
                Key = Key | 1;
                dwflag = dwflag | NativeShellAPI.SHGFI.LINKOVERLAY;
                HasOverlay = true;
            }
            if (item.IsShared)
            {
                Key = Key | 2;
                dwflag = dwflag | NativeShellAPI.SHGFI.ADDOVERLAYS;
                HasOverlay = true;
            }
            if (SelectedIcon)
            {
                Key = Key | 4;
                dwflag = dwflag | NativeShellAPI.SHGFI.OPENICON;
                HasOverlay = true; //not really an overlay, but handled the same
            }
            
            if (imageTable.ContainsKey(Key))
            {
                rVal = (int)imageTable[Key];
            }
            else if (!HasOverlay && !item.IsHidden) //for non-overlay icons, we already have
            {                
                rVal = (int)System.Math.Floor((double)Key / 256); // the right index -- put in table
                imageTable[Key] = rVal;
            }
            else //don't have iconindex for an overlay, get it.
            {
                if (item.IsFileSystem & !item.IsDisk & !item.IsFolder)
                {
                    dwflag = dwflag | NativeShellAPI.SHGFI.USEFILEATTRIBUTES;
                    dwAttr = dwAttr | NativeShellAPI.FILE_ATTRIBUTE.NORMAL;
                }

                PIDL pidlFull = item.PIDLFull;

                NativeShellAPI.SHFILEINFO shfiSmall = new NativeShellAPI.SHFILEINFO();
                NativeShellAPI.SHGetFileInfo(pidlFull.Ptr, dwAttr, ref shfiSmall, NativeShellAPI.cbFileInfo, dwflag | NativeShellAPI.SHGFI.SMALLICON);

                NativeShellAPI.SHFILEINFO shfiLarge = new NativeShellAPI.SHFILEINFO();
                NativeShellAPI.SHGetFileInfo(pidlFull.Ptr, dwAttr, ref shfiLarge, NativeShellAPI.cbFileInfo, dwflag | NativeShellAPI.SHGFI.LARGEICON);

                Marshal.FreeCoTaskMem(pidlFull.Ptr);

                lock (imageTable)
                {
                    rVal = NativeShellAPI.ImageList_ReplaceIcon(smallImageListHandle, -1, shfiSmall.hIcon);
                    NativeShellAPI.ImageList_ReplaceIcon(largeImageListHandle, -1, shfiLarge.hIcon);
                }

                NativeShellAPI.DestroyIcon(shfiSmall.hIcon);
                NativeShellAPI.DestroyIcon(shfiLarge.hIcon);
                imageTable[Key] = rVal;
            }

            if (SelectedIcon)
                item.SelectedImageIndex = rVal;
            else
                item.ImageIndex = rVal;
        }

        public static Icon GetIcon(int index, bool small)
        {
            IntPtr iconPtr;

            if (small)
                iconPtr = NativeShellAPI.ImageList_GetIcon(smallImageListHandle, index, NativeShellAPI.ILD.NORMAL);
            else
                iconPtr = NativeShellAPI.ImageList_GetIcon(largeImageListHandle, index, NativeShellAPI.ILD.NORMAL);

            if (iconPtr != IntPtr.Zero)
            {
                Icon icon = Icon.FromHandle(iconPtr);
                Icon retVal = (Icon)icon.Clone();
                NativeShellAPI.DestroyIcon(iconPtr);
                return retVal;
            }
            else
                return null;
        }

        internal static IntPtr SmallImageList { get { return smallImageListHandle; } }
        internal static IntPtr LargeImageList { get { return largeImageListHandle; } }

        #region Set Small Handle

        internal static void SetSmallImageList(TreeView treeView)
        {
            NativeShellAPI.SendMessage(treeView.Handle, NativeShellAPI.WM.TVM_SETIMAGELIST, TVSIL_NORMAL, smallImageListHandle);
        }

        internal static void SetSmallImageList(ListView listView)
        {
            NativeShellAPI.SendMessage(listView.Handle, NativeShellAPI.WM.LVM_SETIMAGELIST, TVSIL_SMALL, smallImageListHandle);
        }

        #endregion

        #region Set Large Handle

        internal static void SetLargeImageList(ListView listView)
        {
            NativeShellAPI.SendMessage(listView.Handle, NativeShellAPI.WM.LVM_SETIMAGELIST, TVSIL_NORMAL, largeImageListHandle);
        }

        #endregion
    }
}
