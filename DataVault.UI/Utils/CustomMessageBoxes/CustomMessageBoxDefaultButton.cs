using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataVault.UI.Utils.CustomMessageBoxes.Impl;

namespace DataVault.UI.Utils.CustomMessageBoxes
{
    public class CustomMessageBoxDefaultButton
    {
        private List<Action<MessageBoxEx>> _actions = new List<Action<MessageBoxEx>>();
        internal void Apply(MessageBoxEx mbex) { _actions.ForEach(action => action(mbex)); }
        internal CustomMessageBoxDefaultButton(){}

        public static implicit operator CustomMessageBoxDefaultButton(int index)
        {
            return new CustomMessageBoxDefaultButton{_actions = {mbex => mbex.DefaultButtonIndex = index}};
        }

        public static implicit operator CustomMessageBoxDefaultButton(MessageBoxDefaultButton mbdb)
        {
            return int.Parse(mbdb.ToString().Last().ToString());
        }
    }
}