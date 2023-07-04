using Dapper;
using GroceryList.Models;
using GroceryList.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public class DbGroceryRepository : IGroceryRepository
    {
        private readonly string connect;
        private readonly DbProviderFactory factory;
        private readonly IResourceMapper map;
        private readonly ILogger<DbGroceryRepository> logger;

        public DbGroceryRepository(DbProviderFactory dbProviderFactory, IResourceMapper resourceMapper, IConfiguration configuration, ILogger<DbGroceryRepository> groceryLogger)
        {
            factory = dbProviderFactory;
            connect = configuration.GetConnectionString("GroceriesData");
            map = resourceMapper;
            logger = groceryLogger;
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
            var sql = new SqlResourceBuilder(map, "Grocery.SelectCurrent", homeId, logger);
            sql.Append("1 = 1");
            await using var conn = GetConnection(homeId);
            var query = await conn.QueryAsync<GroceryItem>(sql.ToString());

            if (query == null) return Array.Empty<GroceryItem>();
            return query;
        }
        public async Task<GroceryItem?> GetItemAsync(string homeId, string itemId)
        {
            // JACOB: FUTURE IDEA: make these strings only allocate once
            var sql = new SqlResourceBuilder(map, "Grocery.SelectCurrent", homeId, logger);
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

            var sql = new SqlResourceBuilder(map, "Grocery.AddOrUpdate", model.HomeId, logger);
            var select = await map.GetSqlAsync("Grocery.SelectCurrent");
            sql.Replace("SelectQuery", select);
            sql.Append("`item_id` = @Id;");
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
            var sql = new SqlResourceBuilder(map, "Grocery.DeleteCurrent", model.HomeId, logger);
            await using var conn = GetConnection(model.HomeId);
            var count = await conn.ExecuteAsync(sql.ToString(), new { Id = model.Id });
            return count == 0 ? null : model;
        }

        private SqlResourceBuilder GetCheckoutSql(string homeId)
        {

            var sql = new SqlResourceBuilder(map, "Grocery.SelectCurrent", homeId, logger);
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
            // create parameter for each item in list?
            sql.Append(" `item_id` IN(@Ids)");

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
