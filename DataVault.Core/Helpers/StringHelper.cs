using System;
using System.Text;

namespace DataVault.Core.Helpers
{
    public static class StringHelper
    {
        public static String Slice(this String source, int start)
        {
            if (source == null) return String.Empty;

            if (start < 0) start += source.Length;
            if (start >= source.Length) return String.Empty;

            return source.Substring(start);
        }

        public static String Slice(this String source, int start, int end)
        {
            if (source == null) return String.Empty;

            if (start < 0) start += source.Length;
            if (end < 0) end += source.Length;

            if (start >= source.Length) return String.Empty;
            if (start >= end) return String.Empty;
            if (end >= source.Length) return source.Substring(start);

            return source.Substring(start, end - start);
        }

        public static String[] SplitLines(this String s)
        {
            return s.Split(Environment.NewLine.MkArray(), StringSplitOptions.None);
        }
    }
}