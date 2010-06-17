using System;
using System.IO;

namespace DataVault.Core.Api
{
    public interface IBranch : IElement
    {
        IBranch[] GetBranches();
        IBranch[] GetBranchesRecursive();
        IValue[] GetValues();
        IValue[] GetValuesRecursive();

        IBranch GetBranch(VPath vpath);
        IValue GetValue(VPath vpath);

        IBranch CreateBranch(VPath vpath);
        IValue CreateValue(VPath vpath, Func<Stream> content);

        IBranch GetOrCreateBranch(VPath vpath);
        IValue GetOrCreateValue(VPath vpath, Func<Stream> content);

        IBranch AttachBranch(IBranch branch);
        IValue AttachValue(IValue value);

        IBranch ImportBranch(IBranch branch);
        IBranch ImportBranch(IBranch branch, CollisionHandling collisionHandling);
        IValue ImportValue(IValue value);
        IValue ImportValue(IValue value, bool overwrite);

        new IBranch CacheInMemory();
        new IBranch Clone();
    }
}