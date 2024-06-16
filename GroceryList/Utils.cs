//using Humanizer.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;

namespace GroceryList
{
    internal static class Utils
    {
        public static string GetNewId()
        {
            return "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        }

        public static string GetNewUuid(){
            return Guid.NewGuid().ToString("d").ToLower();
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

        public static string GetConnectionWithSecrets(this IConfiguration self, string key)
        {
            var temp = self.GetConnectionString(key)
                .ReplaceSecret(self, "GROCERY_DB_USER")
                .ReplaceSecret(self, "GROCERY_DB_PASS");
            return temp;
        } // END GetConnectionWithSecrets

        public static string ReplaceSecret(this string self, IConfiguration conf, string key)
        {
            var toReplace = $"%{key}%";
            if (!self.Contains(toReplace, StringComparison.Ordinal))
                return self;

            var secret = conf.GetValue<string>(key);
            return self.Replace(toReplace, secret ?? string.Empty);
        } // END ReplaceSecret
    }
}
