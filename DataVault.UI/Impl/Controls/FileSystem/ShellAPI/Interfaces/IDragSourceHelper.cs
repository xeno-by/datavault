using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DataVault.UI.Impl.Controls.FileSystem.ShellAPI.Interfaces
{
    [ComImport]
    [GuidAttribute("DE5BF786-477A-11d2-839D-00C04FD918D0")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDragSourceHelper
    {
        // Initializes the drag-image manager for a windowless control
        [PreserveSig]
        Int32 InitializeFromBitmap(
            ref NativeShellAPI.SHDRAGIMAGE pshdi,
            IntPtr pDataObject);

        // Initializes the drag-image manager for a control with a window
        [PreserveSig]
        Int32 InitializeFromWindow(
            IntPtr hwnd,
            ref NativeShellAPI.POINT ppt,
            IntPtr pDataObject);
    }
}
