using System;
using System.Data.Common;
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

            CreateDb(conn);

            CreateTables(conn, homeId);
        }

        private static void CreateDb(DbConnection conn)
        {
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();

            // database(s) / schema

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            // homes table
            RunSql(assembly, conn, "home");
            // users table
            RunSql(assembly, conn, "user");
        }

        private static void RunSql(System.Reflection.Assembly assembly, DbConnection conn, string fileName)
        {
            using var file = assembly.GetManifestResourceStream($"GroceryList.MySQLSetup.SQL.{fileName}.sql");
            if (file == null) return;

            using var rdr = new StreamReader(file);
            var sql = rdr.ReadToEnd();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static void CreateTables(DbConnection conn, string homeId)
        {
            // home current list table

            // home trips table (readonly JSON)
        }

        //`id`
        private const string _sqlAddHome = @"INSERT INTO `home`
    (`slug`, `label`, `created_on`, `created_by`, `created_meta`)
VALUES (@Slug, @Title, @CreatedOn, @CreatedBy, @CreatedMeta);"; //SELECT LAST_INSERT_ID() Id;
        private static string AddHome(DbConnection conn, string title, string userName, string userMeta, string? homeId = null)
        {
            // if we're adding a home from somewhere else that's not in the database
            if (string.IsNullOrWhiteSpace(homeId))
            {
                homeId = Ulid.NewUlid().ToString();
            }
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _sqlAddHome;
            
            _ = cmd.AddParm("@Id", homeId, System.Data.DbType.String, 50);
            _ = cmd.AddParm("@Label", title, System.Data.DbType.String, 100);
            _ = cmd.AddParm("@CreatedOn", DateTimeOffset.Now.UtcDateTime, System.Data.DbType.DateTime);
	        _ = cmd.AddParm("@CreatedBy", homeId, System.Data.DbType.String, 200);
            _ = cmd.AddParm("@CreatedMeta", homeId, System.Data.DbType.String, 1000);

            // check state?
            conn.Open();
            if (cmd.ExecuteNonQuery() > 0) return homeId;

            return string.Empty;
        }
        private const string _sqlAddUser = @"INSERT INTO `user` (
	`name` varchar(100) NOT NULL,
	`email` varchar(200) NOT NULL,
	`pass_hash` varbinary(512) NOT NULL,
	`auth_ep` varchar(400),
	`last_token` blob,
	`last_logged_in` datetime,
	`created_on` datetime NOT NULL,
	`created_by` varchar(100) NOT NULL,
	`modified_on` datetime,
	`modified_by` varchar(100),
	`active`
    (`slug`, `label`, `created_on`, `created_by`, `created_meta`)
VALUES (@Slug, @Title, @CreatedOn, @CreatedBy, @CreatedMeta);"; //SELECT LAST_INSERT_ID() Id;
        private static string AddUser(DbConnection conn, string title, string userName, string userMeta)
        {
            var homeId = Ulid.NewUlid().ToString();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _sqlAddUser;

            _ = cmd.AddParm("@Id", homeId, System.Data.DbType.String, 50);
            _ = cmd.AddParm("@Label", title, System.Data.DbType.String, 100);
            _ = cmd.AddParm("@CreatedOn", DateTimeOffset.Now.UtcDateTime, System.Data.DbType.DateTime);
            _ = cmd.AddParm("@CreatedBy", homeId, System.Data.DbType.String, 200);
            _ = cmd.AddParm("@CreatedMeta", homeId, System.Data.DbType.String, 1000);

            // check state?
            conn.Open();
            if (cmd.ExecuteNonQuery() > 0) return homeId;

            return string.Empty;
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
