using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroceryList.MySQLSetup
{
    internal static class SetupUtility
    {
        public static void Setup(CliOptions options, DbProviderFactory providerFactory)
        {

            using var conn = providerFactory.CreateConnection();
            if (conn == null)
            {
                Console.Error.WriteLine("No Connection Generated!");
                return;
            }
            var homeId = options.HomeId;
            if (string.IsNullOrWhiteSpace(homeId))
                throw new ArgumentNullException(nameof(homeId), "Home ID is REQUIRED");

            if (homeId.Contains("gener", StringComparison.OrdinalIgnoreCase))
            {
                homeId = Guid.NewGuid().ToString();
                options.HomeId = homeId;
            }

            CreateDb(conn, homeId);

            CreateTables(conn, homeId);
        }

        private static void CreateDb(DbConnection conn, string homeId)
        {
            // database(s) / schema

            // homes table
            // users table
        }

        private static void CreateTables(DbConnection conn, string homeId)
        {
            // home current list table

            // home trips table (readonly JSON)

        }
    }
}
