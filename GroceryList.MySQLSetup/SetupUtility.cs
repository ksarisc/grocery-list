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

        //`id`
        private const string _sqlAddHome = @"INSERT INTO `home` (`slug`, `label`, `created_on`, `created_by`, `created_meta`)
VALUES ();

SELECT LAST_INSERT_ID();";
        private static string AddHome(DbConnection conn)
        {
            var homeId = Ulid.NewUlid().ToString();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _sqlAddHome;
            
            _ = cmd.AddParm("@Slug", homeId, System.Data.DbType.String, 30);
            _ = cmd.AddParm("@label", homeId, System.Data.DbType.String, 50);
            _ = cmd.AddParm("@created_on", DateTimeOffset.Now.UtcDateTime, System.Data.DbType.DateTime);
	        _ = cmd.AddParm("@created_by", homeId, System.Data.DbType.String, 100);
            _ = cmd.AddParm("@created_meta", homeId, System.Data.DbType.String, 1000);

            // check state?
            conn.Open();
            if (cmd.ExecuteNonQuery() > 0) return homeId;

            return null;
        }

        private static DbParameter AddParm(this DbCommand cmd, string name, object? value, System.Data.DbType? dbType = null, int? length = null)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            if (value != null) p.Value = value;
            else p.Value = DBNull.Value;

            //p.DbType = System.Data.DbType.String;
            if (dbType != null) p.DbType = dbType.Value;
            //p.Precision = 30;

            cmd.Parameters.Add(p);
            return p;
        }
    }
}
