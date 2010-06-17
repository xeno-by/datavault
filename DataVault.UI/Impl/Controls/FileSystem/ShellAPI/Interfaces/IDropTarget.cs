using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DataVault.UI.Impl.Controls.FileSystem.ShellAPI.Interfaces
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000122-0000-0000-C000-000000000046")]
    internal interface IDropTarget
    {
        // Determines whether a drop can be accepted and its effect if it is accepted
        [PreserveSig]
        Int32 DragEnter(
            IntPtr pDataObj, 
            NativeShellAPI.MK grfKeyState,
            NativeShellAPI.POINT pt, 
            ref DragDropEffects pdwEffect);

        // Provides target feedback to the user through the DoDragDrop function
        [PreserveSig]
        Int32 DragOver(
            NativeShellAPI.MK grfKeyState,
            NativeShellAPI.POINT pt, 
            ref DragDropEffects pdwEffect);

        // Causes the drop target to suspend its feedback actions
        [PreserveSig]
        Int32 DragLeave();

        // Drops the data into the target window
        [PreserveSig]
        Int32 DragDrop(
            IntPtr pDataObj,
            NativeShellAPI.MK grfKeyState,
            NativeShellAPI.POINT pt,
            ref DragDropEffects pdwEffect);
    }
}
