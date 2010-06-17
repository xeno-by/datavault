using System;
using DataVault.Core.Api;

namespace DataVault.Core.Helpers
{
    public static class VPathHelper
    {
        public static String ToZipPathDir(this String vpath)
        {
            return ((VPath)vpath).ToZipPathDir();
        }

        public static String ToZipPathDir(this VPath vpath)
        {
            var s = (String)vpath; 
            return s.Substring(1).Replace(@"\", "/") + @"\";
        }

        public static String ToZipPathFile(this String vpath)
        {
            return ((VPath)vpath).ToZipPathFile();
        }

        public static String ToZipPathFile(this VPath vpath)
        {
            var s = (String)vpath;
            s = s.Substring(1).Replace(@"\", "/");
            if (!s.Contains("/")) s = "/" + s;
            return s;
        }

        public static String ToFsPathDir(this String vpath)
        {
            return ((VPath)vpath).ToFsPathDir();
        }

        public static String ToFsPathDir(this VPath vpath)
        {
            return ((String)vpath + @"\");
        }

        public static String ToFsPathFile(this String vpath)
        {
            return ((VPath)vpath).ToFsPathFile();
        }

        public static String ToFsPathFile(this VPath vpath)
        {
            return (String)vpath;
        }
    }
}