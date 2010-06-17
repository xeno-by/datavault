using System;
using System.Collections.Generic;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using NUnit.Framework;

namespace DataVault.Playground
{
    public class RevisionHistory
    {
        private List<UInt64> _revisions = new List<UInt64>();
        private List<int> _dirs = new List<int>();
        private Guid? _lastVaultId;

        public void BeforeInv(IVault vault)
        {
            vault.AssertNotNull();
            (_lastVaultId == null || _lastVaultId == vault.Id).AssertTrue();
            _lastVaultId = vault.Id;

            _revisions.Add(vault.Revision);
            _dirs.Add(0);
        }

        public void BeforeMutate(IVault vault)
        {
            vault.AssertNotNull();
            (_lastVaultId == null || _lastVaultId == vault.Id).AssertTrue();
            _lastVaultId = vault.Id;

            _revisions.Add(vault.Revision);
            _dirs.Add(1);
        }

        public void Verify(IVault vault)
        {
            var auxValuesAdded = false;
            if (_lastVaultId != null)
            {
                _revisions.Add(vault.Revision);
                _dirs.Add(int.MaxValue);
                auxValuesAdded = true;
            }

            try
            {
                for (var i = 0; i < _revisions.Count - 1; ++i)
                {
                    if (_dirs[i] == 0)
                    {
                        if (_revisions[i + 1] >= _revisions[i]) continue;
                    }
                    else if (_dirs[i] == 1)
                    {
                        if (_revisions[i + 1] > _revisions[i]) continue;
                    }

                    Func<UInt64, int, String> fmt = (rev, dir) =>
                        String.Format("{0}{1}", rev, dir == 1 ? "+" : dir == 0 ? "=" : "?");

                    throw new AssertionException(String.Format(
                        "Revision history '{0}' is invalid at '{1}, {2}'.",
                        _revisions.Zip(_dirs, fmt).StringJoin(),
                        fmt(_revisions[i], _dirs[i]),
                        _revisions[i + 1]));
                }
            }
            finally 
            {
                if (auxValuesAdded)
                {
                    _revisions.RemoveAt(_revisions.Count - 1);
                    _dirs.RemoveAt(_dirs.Count - 1);
                }
            }
        }
    }
}