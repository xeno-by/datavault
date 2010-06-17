using System;
using System.IO;
using System.Linq;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Impl.Controls.FileSystem;
using DataVault.UI.Impl.Controls.FileSystem.ShellAPI;

namespace DataVault.UI.Impl.VaultFormatz
{
    internal static class PathHelper
    {
        public static String ResolveShellPathAsFileSystemPath(this String shellPath, ShellBrowser shellBrowser)
        {
            try
            {
                String resolved;
                if (Path.IsPathRooted(shellPath))
                {
                    resolved = shellPath;
                }
                else
                {
                    var parent = shellBrowser.CurrentDirectory;
                    parent.AssertNotNull();

                    Func<ShellItem, String> reallyRealPath = si =>
                    {
                        var realPath = ShellItem.GetRealPath(si);
                        Func<Func<String>, String> neverFail = f => { try { return f(); } catch { return null; } };
                        realPath = neverFail(() => Path.GetFullPath(realPath));

                        if (realPath == null)
                        {
                            var desktop = shellBrowser.ShellBrowserComponent.DesktopItem;
                            if (si == desktop)
                            {
                                realPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                            }
                            else if (si.Text == shellBrowser.ShellBrowserComponent.MyComputerName)
                            {
                                realPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                            }
                            else
                            {
                                // todo. possibly map other bizarre folders
                                AssertionHelper.Fail();
                            }
                        }

                        realPath.AssertNotNull();
                        return realPath;
                    };

                    // strategy 1. match children of current parent with the fpath
                    var match = parent.Cast<ShellItem>().SingleOrDefaultDontCrash(si => si.Text == shellPath);
                    if (match != null)
                    {
                        match.IsFileSystem.AssertTrue();
                        match.IsLink.AssertFalse();
                        resolved = reallyRealPath(match);
                    }
                    // strategy 2. resolve fpath as [parent's real path] + fpath
                    else
                    {
                        var parentResolved = reallyRealPath(parent);
                        resolved = Path.Combine(parentResolved, shellPath);
                    }
                }

                resolved.AssertNotNull();
                Path.IsPathRooted(resolved).AssertTrue();

                // canonicalize the path
                resolved = Path.GetFullPath(resolved);
                return resolved;
            }
            catch (Exception)
            {
                // i know this is bad, but in this case user experience >> robustness
                return null;
            }
        }

        public static void EnsureDirectoryExists(this String fsPath)
        {
            Path.IsPathRooted(fsPath).AssertTrue();

            if (!Directory.Exists(fsPath))
            {
                Directory.CreateDirectory(fsPath);
            }
        }
    }
}
