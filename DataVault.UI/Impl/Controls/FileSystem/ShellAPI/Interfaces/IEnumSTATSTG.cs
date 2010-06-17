using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DataVault.UI.Impl.Controls.FileSystem.ShellAPI.Interfaces
{
    [ComImport]
    [Guid("0000000d-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumSTATSTG
    {
        // The user needs to allocate an STATSTG array whose size is celt.
        [PreserveSig]
        uint Next(
            uint celt,
            [MarshalAs(UnmanagedType.LPArray)]
            out NativeShellAPI.STATSTG[] rgelt,
            out uint pceltFetched);

        [PreserveSig]
        void Skip(uint celt);

        [PreserveSig]
        void Reset();

        [PreserveSig]
        IEnumSTATSTG Clone();
    }
}
