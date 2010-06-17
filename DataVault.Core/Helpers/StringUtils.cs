using System;
using System.Text;

namespace DataVault.Core.Helpers
{
    public static class StringUtils
    {
        public static String Slash(this String s)
        {
            if (!s.EndsWith(@"\")) s += @"\";
            return s;
        }

        public static String Unslash(this String s)
        {
            if (s.EndsWith(@"\")) s = s.Substring(0, s.Length - 1);
            return s;
        }

        // not for reuse for tasks unrelated with its current usages
        public static String DoubleBux(this String s)
        {
            return s + "$$";
        }

        // not for reuse for tasks unrelated with its current usages
        public static String Unbux(this String s)
        {
            return s.Replace("$$$$", "").Replace("$$", "");
        }

        public static String RevSubstring(this String s, int startIndex)
        {
            return s.Substring(0, s.Length - startIndex);
        }

        public static String CommonStart(this String s1, String s2)
        {
            var sb = new StringBuilder();

            var s1e = (s1 ?? String.Empty).GetEnumerator();
            var s2e = (s2 ?? String.Empty).GetEnumerator();

            while (true)
            {
                bool next1 = s1e.MoveNext(), next2 = s2e.MoveNext();
                if (!next1 || !next2)
                {
                    return sb.ToString();
                }
                else
                {
                    if (s1e.Current == s2e.Current)
                    {
                        sb.Append(s1e.Current);
                    }
                    else
                    {
                        return sb.ToString();
                    }
                }
            }
        }
    }
}