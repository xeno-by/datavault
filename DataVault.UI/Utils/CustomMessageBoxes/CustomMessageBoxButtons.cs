using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using DataVault.Core.Helpers;
using DataVault.UI.Utils.CustomMessageBoxes.Impl;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.UI.Utils.CustomMessageBoxes
{
    public class CustomMessageBoxButtons
    {
        private List<Action<MessageBoxEx>> _actions = new List<Action<MessageBoxEx>>();
        internal void Apply(MessageBoxEx mbex) { _actions.ForEach(action => action(mbex)); }
        internal CustomMessageBoxButtons(){}

        public static implicit operator CustomMessageBoxButtons(String s)
        {
            var cmbb = new CustomMessageBoxButtons();

            var s_buttons = s.Split("|".MkArray(), StringSplitOptions.None).Select(s1 => s1.Trim());
            s_buttons.ForEach((s_button, i) =>
            {
                var mbebs = Enum.GetValues(typeof(MessageBoxExButtons)).Cast<MessageBoxExButtons>()
                    .ToDictionary(@enum => @enum.ToString(), @enum => @enum);
                var mbeb = mbebs.Keys.SingleOrDefault(mbeb1 => s_button.Contains("{" + mbeb1 + "}"));
                var systemButton = mbeb == null ? null : (MessageBoxExButtons?)mbebs[mbeb];
                var isCancel = systemButton == MessageBoxExButtons.Cancel;

                var text = systemButton == null ? s_button : s_button.Replace("{" + systemButton + "}", "");
                if (text.IsNotEmpty()) (systemButton == null || systemButton == MessageBoxExButtons.Cancel).AssertTrue();

                cmbb._actions.Add(mbex =>
                {
                    if (text.IsEmpty()) mbex.AddButton(systemButton.AssertCast<MessageBoxExButtons>());
                    else mbex.AddButton(text, "");

                    var btn = mbex.Buttons.Last();
                    btn.Value = (i+1).ToString();
                    btn.IsCancelButton = isCancel;
                });
            });

            return cmbb;
        }

        public static implicit operator CustomMessageBoxButtons(MessageBoxButtons mbb)
        {
            var cmbb = new CustomMessageBoxButtons();

            var buttons = mbb.ToInvariantString();
            while (buttons.IsNotEmpty())
            {
                var mbebs = Enum.GetValues(typeof(MessageBoxExButtons)).Cast<MessageBoxExButtons>()
                    .ToDictionary(@enum => @enum.ToString(), @enum => @enum);
                var suitable = mbebs.Keys.SingleOrDefault(buttons.StartsWith);

                suitable.AssertNotNull();
                buttons = buttons.Substring(suitable.Length);

                var index = cmbb._actions.Count() + 1;
                cmbb._actions.Add(mbex =>
                {
                    mbex.AddButton(mbebs[suitable]);
                    var btn = mbex.Buttons.Last();
                    btn.Value = index.ToString();
                });
            }

            return cmbb;
        }
    }
}