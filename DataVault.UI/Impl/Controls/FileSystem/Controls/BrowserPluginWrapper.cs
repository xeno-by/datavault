using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using DataVault.UI.Impl.Controls.FileSystem.ShellAPI;
using DataVault.UI.Impl.Controls.FileSystem.ShellAPI.Interfaces;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;

namespace DataVault.UI.Impl.Controls.FileSystem.Controls
{
    public class BrowserPluginWrapper : Component
    {
        #region Fields

        private ArrayList columnPlugins;
        private ArrayList viewPlugins;
        private ArrayList contextPlugins;

        #endregion

        public BrowserPluginWrapper()
        {
            columnPlugins = new ArrayList();
            viewPlugins = new ArrayList();
            contextPlugins = new ArrayList();

            LoadPlugins();
        }

        private void LoadPlugins()
        {
            string pluginPath = Application.StartupPath + @"\plugins";

            if (Directory.Exists(pluginPath))
            {
                string[] files = Directory.GetFiles(pluginPath, "*.dll");

                foreach (string file in files)
                {
                    try
                    {
                        Assembly plugin = Assembly.LoadFile(file);

                        Type[] types = plugin.GetTypes();

                        foreach (Type type in types)
                        {
                            try
                            {
                                IBrowserPlugin browserPlugin =
                                    plugin.CreateInstance(type.ToString()) as IBrowserPlugin;

                                if (browserPlugin != null)
                                {
                                    if (browserPlugin is IColumnPlugin)
                                        columnPlugins.Add(browserPlugin);

                                    if (browserPlugin is IViewPlugin)
                                        viewPlugins.Add(browserPlugin);

                                    //if (browserPlugin is IContextPlugin)
                                    //contextPlugins.Add(browserPlugin);
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        #region Properties

        public ArrayList ColumnPlugins { get { return columnPlugins; } }
        public ArrayList ViewPlugins { get { return viewPlugins; } }
        public ArrayList ContextPlugins { get { return contextPlugins; } }

        #endregion
    }

    internal interface IBrowserPlugin
    {
        string Name { get; }
        string Info { get; }
    }

    internal interface IColumnPlugin : IBrowserPlugin
    {
        string[] ColumnNames { get; }
        
        HorizontalAlignment GetAlignment(string columnName);

        string GetFolderInfo(IDirInfoProvider provider, string columnName, ShellItem item);
        string GetFileInfo(IFileInfoProvider provider, string columnName, ShellItem item);
    }

    internal interface IViewPlugin : IBrowserPlugin
    {
        string ViewName { get; }
        Control ViewControl { get; }

        void FolderSelected(IDirInfoProvider provider, ShellItem item);
        void FileSelected(IFileInfoProvider provider, ShellItem item);
        void Reset();
    }

    internal interface IDirInfoProvider
    {
        NativeShellAPI.STATSTG GetDirInfo();
    }

    internal interface IFileInfoProvider
    {
        NativeShellAPI.STATSTG GetFileInfo();
        Stream GetFileStream();
    }

    internal interface IDirectoryInfoProvider2 : IDirInfoProvider
    {
        IStorage GetDirInfo(ShellItem item);
    }

    internal interface IFileInfoProvider2 : IFileInfoProvider
    {
        IStream GetFileInfo(ShellItem item);
        Stream GetFileStream(ShellItem item);
    }
}