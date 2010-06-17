using System;
using System.Windows.Forms;
using DataVault.UI.Utils.CustomMessageBoxes.Impl;

namespace DataVault.UI.Utils.CustomMessageBoxes
{
    public static class CustomMessageBox
    {
        public static CustomMessageBoxDialogResult Show(String text, String caption, CustomMessageBoxButtons buttons)
        {
            return ShowCore(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        public static CustomMessageBoxDialogResult Show(IWin32Window owner, String text, String caption, CustomMessageBoxButtons buttons)
        {
            return ShowCore(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
        }

        public static CustomMessageBoxDialogResult Show(String text, String caption, CustomMessageBoxButtons buttons, CustomMessageBoxIcon icon)
        {
            return ShowCore(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        public static CustomMessageBoxDialogResult Show(IWin32Window owner, String text, String caption, CustomMessageBoxButtons buttons, CustomMessageBoxIcon icon)
        {
            return ShowCore(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        public static CustomMessageBoxDialogResult Show(String text, String caption, CustomMessageBoxButtons buttons, CustomMessageBoxIcon icon, CustomMessageBoxDefaultButton defaultButton)
        {
            return ShowCore(null, text, caption, buttons, icon, defaultButton);
        }

        public static CustomMessageBoxDialogResult Show(IWin32Window owner, String text, String caption, CustomMessageBoxButtons buttons, CustomMessageBoxIcon icon, CustomMessageBoxDefaultButton defaultButton)
        {
            return ShowCore(owner, text, caption, buttons, icon, defaultButton);
        }

        private static CustomMessageBoxDialogResult ShowCore(IWin32Window owner, String text, String caption, CustomMessageBoxButtons buttons, CustomMessageBoxIcon icon, CustomMessageBoxDefaultButton defaultButton)
        {
            var mbex = MessageBoxExManager.CreateMessageBox(Guid.NewGuid().ToString());
            mbex.AllowSaveResponse = false;
            mbex.UseSavedResponse = false;
            mbex.Timeout = -1;

            mbex.Text = text;
            mbex.Caption = caption;
            buttons.Apply(mbex);
            icon.Apply(mbex);
            defaultButton.Apply(mbex);

            var result = mbex.Show(owner);
            return new CustomMessageBoxDialogResult(mbex, result);
        }
    }
}
