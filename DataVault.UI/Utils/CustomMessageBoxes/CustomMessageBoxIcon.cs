using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DataVault.UI.Utils.CustomMessageBoxes.Impl;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers;

namespace DataVault.UI.Utils.CustomMessageBoxes
{
    public class CustomMessageBoxIcon
    {
        private List<Action<MessageBoxEx>> _actions = new List<Action<MessageBoxEx>>();
        internal void Apply(MessageBoxEx mbex) { _actions.ForEach(action => action(mbex)); }
        internal CustomMessageBoxIcon(){}

        public static implicit operator CustomMessageBoxIcon(Icon icon)
        {
            return new CustomMessageBoxIcon{_actions = {mbex => mbex.CustomIcon = icon}};
        }

        public static implicit operator CustomMessageBoxIcon(MessageBoxIcon mbi)
        {
            var mbeis = Enum.GetValues(typeof(MessageBoxExIcon)).Cast<MessageBoxExIcon>()
                .ToDictionary(@enum => @enum.ToString(), @enum => @enum);
            mbeis.ContainsKey(mbi.ToString()).AssertTrue();

            var mbei = mbeis[mbi.ToInvariantString()];
            return new CustomMessageBoxIcon{_actions = {mbex => mbex.Icon = mbei}};
        }
    }
}