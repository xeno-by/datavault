using System;
namespace DataVault.Core.Api
{
    public interface IElement
    {
        IVault Vault { get; }
        IBranch Parent { get; }
        VPath VPath { get; }

        // todo. implement last modified marker (correct and recursive)
//        DateTime? LastModified { get; }

        Guid Id { get; }
        String Name { get; }
        IMetadata Metadata { get; }

        void Delete();
        IElement Rename(String name);

        IElement CacheInMemory();
        IElement Clone();
    }
}