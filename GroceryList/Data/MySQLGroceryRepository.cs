using Dapper;
using GroceryList.Lib;
using GroceryList.Lib.Models;
using GroceryList.Models;
using GroceryList.Models.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public class MySQLGroceryRepository : IGroceryRepository
    {
        private readonly ConcurrentDictionary<string, int> _homes = new(); //StringComparer.OrdinalIgnoreCase);
        private readonly string _connect;
        private readonly ILogger<MySQLGroceryRepository> _log;

        public MySQLGroceryRepository(IConfiguration configuration, ILogger<MySQLGroceryRepository> dataLogger) //IResourceMapper resourceMapper, 
        {
            _connect = configuration.GetConnectionWithSecrets("Main");
            //map = resourceMapper;
            _log = dataLogger;
        }

        private const string _sqlGetHomeId = "SELECT `home_id` FROM `home` WHERE `slug` = @HomeSlug;";
        private async Task<int> GetHomeId(MySqlConnection conn, string homeSlug, CancellationToken cancel)
        {
            int id;
            if (int.TryParse(homeSlug, out id) && id > 0)
            {
                return id;
            }

            // cache the results & check it?
            var homeLower = homeSlug.ToLower();
            if (_homes.TryGetValue(homeLower, out id) && id > 0)
            {
                return id;
            }

            await using var cmd = new MySqlCommand(_sqlGetHomeId, conn);
            var p = new MySqlParameter("@HomeSlug", MySqlDbType.String);
            p.Value = homeSlug;
            var result = await cmd.ExecuteScalarAsync(cancel);
            if (result != null && result != DBNull.Value)
            {
                id = Convert.ToInt32(result);
                _homes.AddOrUpdate(homeLower, id, (s, i) => id);
                return id;
            }
            //var def = new CommandDefinition(_sqlGetHomeId, new {}, cancellationToken: cancel);
            //found = await conn.QueryFirstOrDefaultAsync<int?>(def) ?? 0;
            return 0;
        }

        //private const string _sqlEdit = "";
        private const string _sqlAdd = @"INSERT INTO `{0}_current_list` (
    `home_id`, `name`, `section`, `brand`, `notes`, `price`, `quantity`,`created_on`,`created_tz`, `created_by`,
    `in_cart_on`, `in_cart_tz`, `in_cart_by`, `purchased_on`, `purchased_tz`, `purchased_by`)
VALUES (
    @HomeId, @Name, @Section, @Brand, @Notes, @Price, @Qty, @CreatedOn, @CreatedTz, @CreatedUser,
    @InCartOn, @InCartTz, @InCartUser, @PurchasedOn, @PurchasedTz, @PurchasedUser) 
ON DUPLICATE KEY UPDATE
    `section` = @Section, `brand` = @Brand, `notes` = @Notes, `price` = @Price, `quantity` = @Qty,
    `created_on` = @CreatedOn, `created_tz` = @CreatedTz, `created_by` = @CreatedUser,
    `in_cart_on` = @InCartOn, `in_cart_tz` = @InCartTz, `in_cart_by` = @InCartUser,
    `purchased_on` = @PurchasedOn, `purchased_tz` = @PurchasedTz, `purchased_by` = @PurchasedUser
;";
        public async Task<GroceryItem?> AddAsync(GroceryItem model, CancellationToken cancel)
        {
            try
            {
                await using var conn = new MySqlConnection(_connect);
                await conn.OpenAsync(cancel);

                var homeId = await GetHomeId(conn, model.HomeId, cancel);
                var sql = string.Format(_sqlAdd, homeId);

                var def = new CommandDefinition(sql, model, cancellationToken: cancel);
                return await conn.QueryFirstOrDefaultAsync<GroceryItem>(def);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Grocery.Add ({@model}) ERRORED", model);
            }
            return null;
        }

        private const string _sqlDelete = @"DELETE
FROM `{0}_current_list`
WHERE `item_id` = @ItemId;";
        public async Task<GroceryItem?> DeleteAsync(GroceryItem model, CancellationToken cancel)
        {
            try
            {
                await using var conn = new MySqlConnection(_connect);
                await conn.OpenAsync(cancel);

                var homeId = await GetHomeId(conn, model.HomeId, cancel);
                var sql = string.Format(_sqlDelete, homeId);

                await using var cmd = new MySqlCommand(sql, conn);
                var p = new MySqlParameter("@ItemId", MySqlDbType.Int32);
                p.Value = model.Id;
                var count = await cmd.ExecuteNonQueryAsync(cancel);
                if (count > 0) return model;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Grocery.Add ({@model}) ERRORED", model);
            }
            return null;
        }

        private const string _sqlGetBase = @"SELECT
    `item_id` Id, `home_id` HomeId, `name` Name, `section` Section,
    `brand` Brand, `notes` Notes, `price` Price, `quantity` Qty,
    `created_on` CreatedOn, `created_tz` CreatedTz, `created_by` CreatedUser,
    `in_cart_on` InCartOn, `in_cart_tz` InCartTz, `in_cart_by` InCartUser,
    `purchased_on` PurchasedOn, `purchased_tz` PurchasedTz, `purchased_by` PurchasedUser
FROM `{0}_current_list`";
        private const string _sqlGetById = _sqlGetBase + @"
WHERE `item_id` = @ItemId;";
        public async Task<GroceryItem?> GetItemAsync(string homeSlug, string itemId, CancellationToken cancel)
        {
            try
            {
                await using var conn = new MySqlConnection(_connect);
                await conn.OpenAsync(cancel);

                var homeId = await GetHomeId(conn, homeSlug, cancel);
                var sql = string.Format(_sqlGetById, homeId);

                var def = new CommandDefinition(sql, new { ItemId = itemId }, cancellationToken: cancel);
                return await conn.QueryFirstOrDefaultAsync<SqlGroceryItem>(def);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Grocery.GetOne (Home:{homeSlug})(Item:{itemId}) ERRORED", homeSlug, itemId);
            }
            return null;
        }

        public async Task<IEnumerable<GroceryItem>> GetListAsync(string homeSlug, CancellationToken cancel)
        {
            try
            {
                await using var conn = new MySqlConnection(_connect);
                await conn.OpenAsync(cancel);

                var homeId = await GetHomeId(conn, homeSlug, cancel);
                var sql = string.Format(_sqlGetBase, homeId);

                var def = new CommandDefinition(sql, cancellationToken: cancel);
                return await conn.QueryAsync<SqlGroceryItem>(def);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Grocery.Get (Home:{homeSlug}) ERRORED", homeSlug);
            }
            return [];
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
