using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.UI.Api.Commands.WithUI
{
    public class CommandMapping
    {
        private Dictionary<ToolStripMenuItem, Func<ICommand>> _mapping = new Dictionary<ToolStripMenuItem, Func<ICommand>>();

        public event EventHandler BeforeUIResync;
        public event EventHandler AfterUIResync;

        public void Bind(ToolStripMenuItem menuItem, Func<ICommand> command)
        {
            menuItem.Tag = command;
            menuItem.Click += commandBoundMenuItem_Click;
            _mapping.Add(menuItem, command);
        }

        private void commandBoundMenuItem_Click(Object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var command = ((Func<ICommand>)menuItem.Tag)();
            command.AssertNotNull().Do();
        }

        public void ResyncUI()
        {
            if (BeforeUIResync != null)
                BeforeUIResync(this, EventArgs.Empty);

            foreach (var kvp in _mapping)
            {
                kvp.Key.Enabled = kvp.Value().CanDo();
                PropagateStateRecursive(kvp.Key);
            }

            if (AfterUIResync != null)
                AfterUIResync(this, EventArgs.Empty);
        }

        private void PropagateStateRecursive(ToolStripItem item)
        {
            if (item == null) return;

            if (item is ToolStripDropDownItem)
            {
                var ddi = (ToolStripDropDownItem)item;
                if (!ddi.DropDownItems.IsNullOrEmpty())
                {
                    var anyEnabled = ActualItems(ddi.DropDownItems).Any(SelfEnabled);
                    ddi.Enabled = anyEnabled;
                }
            }

            if (item.Owner != null) PropagateStateRecursive(item.Owner);
            if (item.OwnerItem != null) PropagateStateRecursive(item.OwnerItem);
        }

        private void PropagateStateRecursive(ToolStrip strip)
        {
            if (strip == null) return;

            var anyEnabled = ActualItems(strip.Items).Any(SelfEnabled);
            strip.Enabled = anyEnabled;
        }

        private IEnumerable<ToolStripItem> ActualItems(ToolStripItemCollection items)
        {
            return items.Cast<ToolStripItem>().Where(item => !(item is ToolStripSeparator));
        }

        private bool SelfEnabled(ToolStripItem item)
        {
            var stateEnabled = (int)typeof(ToolStripItem).GetField("stateEnabled", BindingFlags.NonPublic | BindingFlags.Static).AssertNotNull().GetValue(null);
            var state = (BitVector32)typeof(ToolStripItem).GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).AssertNotNull().GetValue(item);
            return state[stateEnabled] && !item.Name.ToLower().Contains("dummy");
        }
    }
}