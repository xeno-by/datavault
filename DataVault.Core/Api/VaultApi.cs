using System;
using DataVault.Core.Impl.Fs;
using DataVault.Core.Impl.Memory;
using DataVault.Core.Impl.Xml;
using DataVault.Core.Impl.Zip;

namespace DataVault.Core.Api
{
    // todo. stuff to do: configure cache overflow policy for the vault
    // (this concerns deleted branches/values and cached streams/strings)

    public static class VaultApi
    {
        public static IVault OpenZip(String fileName)
        {
            return new ZipVault(fileName);
        }

        public static IVault OpenFs(String dirName)
        {
            return new FsVault(dirName);
        }

        public static IVault OpenInMemory()
        {
            return new InMemoryVault();
        }

        public static IVault OpenXml(String fileName)
        {
            return new XmlVault(fileName);
        }
    }
}