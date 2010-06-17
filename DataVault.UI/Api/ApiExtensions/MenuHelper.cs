using System.Collections.Generic;
using System.Windows.Forms;

namespace DataVault.UI.Api.ApiExtensions
{
    public static class MenuHelper
    {
        private static Dictionary<ToolStripMenuItem, Keys> _savedShortcuts =
            new Dictionary<ToolStripMenuItem, Keys>();

        public static void UnshortcutAndStore(this ToolStrip menu)
        {
            UnshortcutAndStore(menu.Items);
        }

        private static void UnshortcutAndStore(ToolStripItemCollection menuItems)
        {
            foreach(ToolStripItem menuItem in menuItems)
            {
                if (menuItem is ToolStripMenuItem)
                {
                    var tsmi = (ToolStripMenuItem)menuItem;
                    _savedShortcuts[tsmi] = tsmi.ShortcutKeys;
                    tsmi.ShortcutKeys = Keys.None;
                }

                if (menuItem is ToolStripDropDownItem)
                {
                    var tsddi = (ToolStripDropDownItem)menuItem;
                    UnshortcutAndStore(tsddi.DropDownItems);
                }
            }
        }

        public static void EnshortcutAndRestore(this ToolStrip menu)
        {
            EnshortcutAndRestore(menu.Items);
        }

        private static void EnshortcutAndRestore(ToolStripItemCollection menuItems)
        {
            foreach (ToolStripItem menuItem in menuItems)
            {
                if (menuItem is ToolStripMenuItem)
                {
                    var tsmi = (ToolStripMenuItem)menuItem;
                    if (_savedShortcuts.ContainsKey(tsmi))
                    {
                        tsmi.ShortcutKeys = _savedShortcuts[tsmi];
                    }
                }

                if (menuItem is ToolStripDropDownItem)
                {
                    var tsddi = (ToolStripDropDownItem)menuItem;
                    EnshortcutAndRestore(tsddi.DropDownItems);
                }
            }
        }
    }
}