using System;
using System.Windows.Forms;

namespace DataVault.UI.Impl.Helpers
{
    internal static class WinFormsHelpers
    {
        public static void ThreadSafeInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private class Box<T> { public T Value { get; set; } }

        public static T ThreadSafeInvoke<T>(this Control control, Func<T> func)
        {
            if (control.InvokeRequired)
            {
                Action<Box<T>> action = box1 =>
                {
                    box1.Value = func();
                };

                var box = new Box<T>();
                control.Invoke(action, box);
                return box.Value;
            }
            else
            {
                return func();
            }
        }
    }
}
