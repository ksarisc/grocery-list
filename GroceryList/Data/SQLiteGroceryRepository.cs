using Dapper;
using Devart.Data.SQLite;
using GroceryList.Lib;
using GroceryList.Lib.Models;
using GroceryList.Models.Data;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public sealed class SQLiteGroceryRepository : IGroceryRepository
    {
        private record HomeData(int HomeId, string Slug, string DbFile, string Connect);

        // for SQlite this will be different files for different homes (don't forget to enable WAL)
        private static readonly ConcurrentDictionary<string, HomeData> _homes = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _mainConn, _dbPath;
        private readonly ILogger _log;

        public SQLiteGroceryRepository(IConfiguration configuration) //, ILogger<SQLiteGroceryRepository> dataLogger) //IResourceMapper resourceMapper, 
        {
            //_log = Log.ForContext<SQLiteGroceryRepository>();
            _log = Log.Logger;

            // this should have the path where database files are stored
            _mainConn = configuration.GetConnectionWithSecrets("Main");
            var sb = new SQLiteConnectionStringBuilder(_mainConn);
            sb.ReadOnlyDatabase = true;
            sb.Password = "TestPassword!"; //configuration.Get<string>("MainDbPassword");
            //map = resourceMapper;
            _dbPath = configuration.Get;
            //_log = dataLogger;
        }

        private const string _sqlGetHomeId = "SELECT \"home_id\", \"db_file\" FROM \"home\" WHERE \"slug\" = @HomeSlug;";
        private HomeData GetHome(string homeSlug)
        {
            // quicker lookup for int?
            //if (int.TryParse(homeSlug, out var id) && id > 0)
            //{
            //    return id;
            //}

            if (homeSlug.Length < 10 || homeSlug.Length > 150)
            {
                _log.Warning("GetDbFile abnormal slug: {homeSlug}", homeSlug);
                throw new ArgumentOutOfRangeException(nameof(homeSlug), "The specified `homeSlug` is INVALID");
            }

            // cache the results & check it?
            if (_homes.TryGetValue(homeSlug, out var data) && data.HomeId > 0)
            {
                return data;
            }

            var conn = new SQLiteConnection(_mainConn);
            conn.Open();
            //using var wal = new SQLiteCommand("PRAGMA journal_mode = 'wal'", conn);
            //wal.ExecuteNonQuery();
            using var cmd = new SQLiteCommand(_sqlGetHomeId, conn);
            var p = new SQLiteParameter("@HomeSlug", SQLiteType.Text);
            p.Value = homeSlug;
            // if database password, salt, and/or path is stored common datasource (might be nice to use key-value store), this will need to be ExecuteReader
            var result = cmd.ExecuteScalar();
            if (null == result || DBNull.Value == result)
            {
                throw new ArgumentOutOfRangeException(nameof(homeSlug), $"The specified `homeSlug` was NOT found: {homeSlug}");
            }

            var id = Convert.ToInt32(result);
            var file = Path.Combine(_dbPath, $"home_{id}.db");
            if (!File.Exists(file))
            {
                CreateDb(file);
            }

            // setup the connection (get database from cache or build)
            var connect = $"Data Source={file};";
            //Password=myPassword;
            //Pooling=True;

            // build the object
            data = new HomeData(id, homeSlug, file, connect);
            _homes.AddOrUpdate(homeSlug, data, (s, i) => data);
            return data;
        }

        private void CreateDb(string fileName)
        {
            throw new NotImplementedException();
        }

        private SQLiteConnection Connect(string homeSlug)
        {
            var dbFile = GetDbFile(homeSlug);

            // get the home's connection
            var conn = new SQLiteConnection(connect);
            conn.Open();
            using var wal = new SQLiteCommand("PRAGMA journal_mode = 'wal'", conn);
            wal.ExecuteNonQuery();
            return conn;
        }

        //private const string _sqlEdit = "";
        private const string _sqlAdd = @"INSERT INTO ""current_list"" (
    ""home_id"", ""name"", ""section"", ""brand"", ""notes"", ""price"", ""quantity"",""created_on"",""created_tz"", ""created_by"",
    ""in_cart_on"", ""in_cart_tz"", ""in_cart_by"", ""purchased_on"", ""purchased_tz"", ""purchased_by"")
VALUES (
    @HomeId, @Name, @Section, @Brand, @Notes, @Price, @Qty, @CreatedOn, @CreatedTz, @CreatedUser,
    @InCartOn, @InCartTz, @InCartUser, @PurchasedOn, @PurchasedTz, @PurchasedUser) 
ON DUPLICATE KEY UPDATE
    ""section"" = @Section, ""brand"" = @Brand, ""notes"" = @Notes, ""price"" = @Price, ""quantity"" = @Qty,
    ""created_on"" = @CreatedOn, ""created_tz"" = @CreatedTz, ""created_by"" = @CreatedUser,
    ""in_cart_on"" = @InCartOn, ""in_cart_tz"" = @InCartTz, ""in_cart_by"" = @InCartUser,
    ""purchased_on"" = @PurchasedOn, ""purchased_tz"" = @PurchasedTz, ""purchased_by"" = @PurchasedUser
;";
        public Task<GroceryItem?> AddAsync(GroceryItem model, CancellationToken cancel)
        {
            //return Task.Run()
            try
            {
                using var conn = Connect(model.HomeId);

                var def = new CommandDefinition(_sqlAdd, model, cancellationToken: cancel);
                var result = conn.QueryFirstOrDefault<GroceryItem>(def);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Grocery.Add ({@model}) ERRORED", model);
            }
            return Task.FromResult<GroceryItem?>(null);
        }

        private const string _sqlDelete = @"DELETE
FROM ""{0}_current_list""
WHERE ""item_id"" = @ItemId;";
        public Task<GroceryItem?> DeleteAsync(GroceryItem model, CancellationToken cancel)
        {
            try
            {
                using var conn = Connect(model.HomeId);

                using var cmd = new SQLiteCommand(_sqlDelete, conn);
                var p = new SQLiteParameter("@ItemId", SQLiteType.Int32);
                p.Value = model.Id;
                var count = cmd.ExecuteNonQuery();
                if (count > 0) return Task.FromResult(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Grocery.Add ({@model}) ERRORED", model);
            }
            return Task.FromResult<GroceryItem?>(null);
        }

        private const string _sqlGetBase = @"SELECT
    ""item_id"" Id, ""home_id"" HomeId, ""name"" Name, ""section"" Section,
    ""brand"" Brand, ""notes"" Notes, ""price"" Price, ""quantity"" Qty,
    ""created_on"" CreatedOn, ""created_tz"" CreatedTz, ""created_by"" CreatedUser,
    ""in_cart_on"" InCartOn, ""in_cart_tz"" InCartTz, ""in_cart_by"" InCartUser,
    ""purchased_on"" PurchasedOn, ""purchased_tz"" PurchasedTz, ""purchased_by"" PurchasedUser
FROM ""current_list""";
        private const string _sqlGetById = _sqlGetBase + @"
WHERE ""item_id"" = @ItemId;";
        public Task<GroceryItem?> GetItemAsync(string homeSlug, string itemId, CancellationToken cancel)
        {
            try
            {
                using var conn = Connect(homeSlug);
                var def = new CommandDefinition(_sqlGetById, new { ItemId = itemId }, cancellationToken: cancel);
                var result = conn.QueryFirstOrDefault<GroceryItem>(def);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Grocery.GetOne (Home:{homeSlug})(Item:{itemId}) ERRORED", homeSlug, itemId);
            }
            return Task.FromResult<GroceryItem?>(null);
        }

        public Task<IEnumerable<GroceryItem>> GetListAsync(string homeSlug, CancellationToken cancel)
        {
            try
            {
                using var conn = Connect(homeSlug);
                var def = new CommandDefinition(_sqlGetBase, cancellationToken: cancel);
                var result = conn.Query<GroceryItem>(def);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Grocery.Get (Home:{homeSlug}) ERRORED", homeSlug);
            }
            return Task.FromResult<IEnumerable<GroceryItem>>([]);
        }

        public Task<IEnumerable<GroceryItem>> CheckoutAsync(string homeId, List<string> checkoutItemIds, string? storeName, CancellationToken cancel)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<GroceryItem>> GetCheckoutAsync(string homeId, CancellationToken cancel)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<GroceryTrip>> GetTripsAsync(string homeId, CancellationToken cancel)
        {
            throw new System.NotImplementedException();
        }
    }
}
