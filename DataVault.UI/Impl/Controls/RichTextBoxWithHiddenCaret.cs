using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DataVault.UI.Impl.Controls
{
    internal class RichTextBoxWithHiddenCaret : RichTextBox
    {
        [DllImport("user32.dll", EntryPoint = "HideCaret")]
        public static extern bool HideCaret(IntPtr hwnd);

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            HideCaret(this.Handle);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            HideCaret(this.Handle);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            HideCaret(this.Handle);
        }
    }
}