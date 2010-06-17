using System;
using System.Linq;
using System.Windows.Forms;
using DataVault.UI.Utils.CustomMessageBoxes.Impl;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Helpers;

namespace DataVault.UI.Utils.CustomMessageBoxes
{
    public class CustomMessageBoxDialogResult
    {
        private readonly MessageBoxEx _mbex;
        private readonly String _result;
        internal CustomMessageBoxDialogResult(MessageBoxEx mbex, String result){ _mbex = mbex; _result = result; }

        public static implicit operator int(CustomMessageBoxDialogResult result)
        {
            int index;
            int.TryParse(result._result, out index).AssertTrue();
            return index;
        }

        public static implicit operator DialogResult(CustomMessageBoxDialogResult result)
        {
            var dr = (DialogResult?)result;
            dr.AssertNotNull();
            return (DialogResult)dr;
        }

        public static implicit operator DialogResult?(CustomMessageBoxDialogResult result)
        {
            var index = (int)result;
            var button = result._mbex.Buttons[index];
            if (button.SystemButton == null) return null;

            var mbeb = (MessageBoxExButtons)button.SystemButton;
            var drs = Enum.GetValues(typeof(DialogResult)).Cast<DialogResult>()
                .ToDictionary(@enum => @enum.ToString(), @enum => @enum);
            var suitable = drs.Keys.SingleOrDefault(dr => dr == mbeb.ToInvariantString());

            if (suitable == null) return null;
            return drs[suitable];
        }
    }
}