using Dapper;
using GroceryList.Lib;
using GroceryList.Lib.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GroceryList.Db
{
	internal class HomeDbRepository
	{
		private readonly string _connect;
		private readonly DbProviderFactory _db;
		private readonly IResourceMapper _map;
		private readonly ILogger<HomeDbRepository> _log;

		public HomeDbRepository(DbProviderFactory dbProviderFactory, IResourceMapper mapper, ILogger<HomeDbRepository> logger)
		{
			_db = dbProviderFactory;
			_map = mapper;
			_log = logger;
			var connect = _map.GetConnectionWithSecrets("Default");
			if (string.IsNullOrWhiteSpace(connect))
			{
				throw new ArgumentNullException(nameof(mapper), "`Default` connection string MISSING");
			}
			_connect = connect;
		}

		public Task<bool> HomeExistsAsync(string homeSlug, CancellationToken cancel)
		{
		}

		public Task<Lib.Models.Home?> AddHomeAsync(Lib.Models.Home home, CancellationToken cancel)
		{
		}

		private const string getHome = "Global.SelectHomeOne";
		//public Task<Models.Home?> GetHomeAsync(string homeId);
		public async Task<Lib.Models.Home?> GetHomeAsync(string homeSlug, CancellationToken cancel)
		{
			var sql = await _map.GetSqlAsync(getHome);
			if (sql == null)
			{
				//var builder = new SqlResourceBuilder(_map, "Global.SelectHome", homeId, _log);
				//builder.Append("`item_id` = @ItemId;");

				sql = _map.SetSql(getHome, builder.ToString());
			}

			if (!int.TryParse(homeSlug, out var homeId)) homeId = 0;

			await using var conn = _db.CreateConnection();
			conn.ConnectionString = _connect;
			var parms = new DynamicParameters();
			parms.Add("HomeSlug", homeSlug, System.Data.DbType.String);
			parms.Add("HomeId", homeId, System.Data.DbType.Int32);
			return await conn.QueryFirstOrDefaultAsync<Lib.Models.Home>(new CommandDefinition(sql, parms, cancellationToken: cancel));
		}

		/*public async Task<IEnumerable<Lib.Models.HomeSecurity>> GetHomeSecurityAsync(string homeId, CancellationToken cancel)
		{
`home_security` (
    `id` INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `home_id` INT NOT NULL,
    `details` JSON NOT NULL,
    `created_on` datetime NOT NULL,
    `created_user` VARCHAR(50) NOT NULL,
		}*/
	}
}
