using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace GroceryList.MySQLSetup
{
    internal static class DataFilesLoader
    {
        public static void Load(CliOptions options, DbProviderFactory providerFactory)
        {
            if (string.IsNullOrWhiteSpace(options.DataFilesPath))
                return;
            if (!Directory.Exists(options.DataFilesPath))
                return;
            if (string.IsNullOrWhiteSpace(options.Connection))
                return;

            Console.WriteLine("Loading previous database files from `{0}`", options.DataFilesPath);

            using var connect = providerFactory.CreateConnection();
            if (connect == null)
            {
                Console.Error.WriteLine("No Connection Generated!");
                return;
            }

            connect.ConnectionString = options.Connection;
            foreach (var homeDir in Directory.GetDirectories(options.DataFilesPath, "*", SearchOption.TopDirectoryOnly))
            {
                LoadHome(connect, homeDir);
            }
        }

        private static void LoadHome(DbConnection conn, string homeFolder)
        {
        }
    }
}
