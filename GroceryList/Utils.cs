using System;

namespace GroceryList
{
    internal static class Utils
    {
        public static string GetNewId()
        {
            return "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        }

        /// SOURCE: https://stackoverflow.com/questions/5116977/how-to-check-the-os-version-at-runtime-e-g-on-windows-or-linux-without-using
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return p == 4 || p == 6 || p == 128;
            }
        }
    }
}
