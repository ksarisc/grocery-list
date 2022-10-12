using Dapper;
using GroceryList.Models;
using GroceryList.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public class DbGroceryRepository : IGroceryRepository
    {
        //         private const string sqlCurrentSelect = @"SELECT `id` Id, `home_id` HomeId, `name` Name, `brand` Brand, `notes` Notes,
        //     `price` Price, `qty` Qty, `created_time` CreatedTime, `created_user` CreatedUser, `in_cart_time` InCartTime,
        //     `in_cart_user` InCartUser, `purchased_time` PurchasedTime, `purchased_user` PurchasedUser
        // FROM `grocery_list_current` WHERE ";
        //         private const string sqlCurrent = sqlCurrentSelect + "1 = 1;";
        //         private const string sqlGetItem = sqlCurrentSelect + "ItemId = @ItemId";
        //         private const string sqlSetItem = @"";
        //         private const string sqlDeleteItem = "";
        //         private const string sqlGetCheckout = "";

        private readonly string connect;
        private readonly DbProviderFactory factory;
        private readonly IResourceMapper map;

        public DbGroceryRepository(DbProviderFactory dbProviderFactory, IResourceMapper resourceMapper, IConfiguration configuration)
        {
            factory = dbProviderFactory;
            connect = configuration.GetConnectionString("GroceriesData");
            map = resourceMapper;
        }

        private DbConnection GetConnection(string homeId)
        {
            var conn = factory.CreateConnection();
            if (conn == null)
                throw new NullReferenceException("Invalid DbConnection Factory result");
            conn.ConnectionString = connect;
            // check for the database existing, create if missing ??
            return conn;
        }

        private async Task CreateHome()
        {
            await Task.Delay(10);
        }

        public async Task<IEnumerable<GroceryItem>> GetListAsync(string homeId)
        {
            var sql = new SqlResourceBuilder(map, "Grocery.SelectCurrent");
            sql.Replace(nameof(homeId), homeId);
            sql.Append("1 = 1");
            await using var conn = GetConnection(homeId);
            var query = await conn.QueryAsync<GroceryItem>(sql.ToString());

            if (query == null) return Array.Empty<GroceryItem>();
            return query;
        }
        public async Task<GroceryItem?> GetItemAsync(string homeId, string itemId)
        {
            var sql = new SqlResourceBuilder(map, "Grocery.SelectCurrent");
            sql.Replace(nameof(homeId), homeId);
            sql.Append("ItemId = @ItemId;");
            await using var conn = GetConnection(homeId);
            return await conn.QueryFirstOrDefaultAsync<GroceryItem>(sql.ToString(),
                new { ItemId = itemId, });
        }

        /// <summary>
        /// Adds/Updates current grocery list with specific item
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public async Task<GroceryItem?> AddAsync(GroceryItem model)
        {

            var sql = new SqlResourceBuilder(map, "Grocery.AddOrUpdate");
            sql.Replace("homeId", model.HomeId);
            var select = await map.GetSqlAsync("Grocery.SelectCurrent");
            sql.Replace("SelectQuery", select);
            sql.Append("ItemId = @ItemId;");
            await using var conn = GetConnection(model.HomeId);
            return await conn.QueryFirstOrDefaultAsync<GroceryItem>(sql.ToString(), model);
        }

        /// <summary>
        /// Remove specific item from current grocery list
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public async Task<GroceryItem?> DeleteAsync(GroceryItem model)
        {
            var sql = new SqlResourceBuilder(map, "Grocery.DeleteCurrent");
            sql.Replace("homeId", model.HomeId);
            await using var conn = GetConnection(model.HomeId);
            var count = await conn.ExecuteAsync(sql.ToString(), new { Id = model.Id });
            return count == 0 ? null : model;
        }

        private SqlResourceBuilder GetCheckoutSql(string homeId)
        {

            var sql = new SqlResourceBuilder(map, "Grocery.SelectCurrent");
            sql.Replace(nameof(homeId), homeId);
            sql.Append("`in_cart_time` = @InCartTime;");
            return sql;
        }

        public async Task<IEnumerable<GroceryItem>> GetCheckoutAsync(string homeId)
        {
            var sql = GetCheckoutSql(homeId);

            await using var conn = GetConnection(homeId);
            var list = await conn.QueryAsync<GroceryItem>(sql.ToString());

            if (list == null) return Array.Empty<GroceryItem>();
            return list;
        }
        public async Task<IEnumerable<GroceryItem>> CheckoutAsync(string homeId, List<string> checkoutItemIds, string? storeName)
        {
            throw new NotImplementedException();

            var sql = GetCheckoutSql(homeId);
            sql.Append(" `id` IN(@Ids)");

            await using var conn = GetConnection(homeId);
            var list = await conn.QueryAsync<GroceryItem>(sql.ToString(), new { Ids = checkoutItemIds, });

            // build trip for checkoutItemIds

            // then delete from current

            if (list == null) return Array.Empty<GroceryItem>();
            return list;
        }

        public async Task<IEnumerable<GroceryTrip>> GetTripsAsync(string homeId)
        {
            throw new NotImplementedException();

            var list = new List<GroceryTrip>();
            // get a list of former trips
            return list;
        }
    }
}
