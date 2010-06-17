using System;
using System.IO;

namespace DataVault.Core.Api
{
    public interface IValue : IElement
    {
        Stream ContentStream { get; }
        String ContentString { get; }
        IValue SetContent(Func<Stream> content);

        new IValue CacheInMemory();
        new IValue Clone();
    }
}
