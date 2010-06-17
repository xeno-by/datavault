using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DataVault.UI.Impl.Controls.FileSystem.ShellAPI.Interfaces
{
    [ComImport]
    [Guid("0000000b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IStorage
    {
        [PreserveSig]
        Int32 CreateStream(
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            NativeShellAPI.STGM grfMode,
            int reserved1,
            int reserved2,
            out IntPtr ppstm);

        [PreserveSig]
        Int32 OpenStream(
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            IntPtr reserved1,
            NativeShellAPI.STGM grfMode,
            int reserved2,
            out IntPtr ppstm);

        [PreserveSig]
        Int32 CreateStorage(
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            NativeShellAPI.STGM grfMode,
            int reserved1,
            int reserved2,
            out IntPtr ppstg);

        [PreserveSig]
        Int32 OpenStorage(
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            IStorage pstgPriority,
            NativeShellAPI.STGM grfMode,
            IntPtr snbExclude,
            int reserved,
            out IntPtr ppstg);

        [PreserveSig]
        Int32 CopyTo(
            int ciidExclude,
            ref Guid rgiidExclude,
            IntPtr snbExclude,
            IStorage pstgDest);

        [PreserveSig]
        Int32 MoveElementTo(
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            IStorage pstgDest,
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsNewName,
            NativeShellAPI.STGMOVE grfFlags);

        [PreserveSig]
        Int32 Commit(
            NativeShellAPI.STGC grfCommitFlags);

        [PreserveSig]
        Int32 Revert();

        [PreserveSig]
        Int32 EnumElements(
            int reserved1,
            IntPtr reserved2,
            int reserved3,
            out IntPtr ppenum);

        [PreserveSig]
        Int32 DestroyElement(
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName);

        [PreserveSig]
        Int32 RenameElement(
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsOldName,
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsNewName);

        [PreserveSig]
        Int32 SetElementTimes(
            [MarshalAs(UnmanagedType.LPWStr)]
            string pwcsName,
            NativeShellAPI.FILETIME pctime,
            NativeShellAPI.FILETIME patime,
            NativeShellAPI.FILETIME pmtime);

        [PreserveSig]
        Int32 SetClass(
            ref Guid clsid);

        [PreserveSig]
        Int32 SetStateBits(
            int grfStateBits,
            int grfMask);

        [PreserveSig]
        Int32 Stat(
            out NativeShellAPI.STATSTG pstatstg,
            NativeShellAPI.STATFLAG grfStatFlag);
    }
}
