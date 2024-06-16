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

namespace GroceryList.Db;

public class DbGroceryRepository : IGroceryRepository
{
    private readonly string _connect;
    private readonly DbProviderFactory _db;
    private readonly IResourceMapper _map;
    private readonly ILogger<DbGroceryRepository> _log;

    public DbGroceryRepository(DbProviderFactory dbProviderFactory, IResourceMapper mapper, ILogger<DbGroceryRepository> logger)
    {
        _db = dbProviderFactory;
        _map = mapper;
        _log = logger;
        // TODO: probably should make this less verbose
        _connect = _map.GetConnectionWithSecrets("Groceries"); //GroceriesData
    }

    private DbConnection GetConnection() //string homeId)
    {
        var conn = _db.CreateConnection();
        if (conn == null)
            throw new NullReferenceException("Invalid DbConnection Factory result");
        conn.ConnectionString = _connect;
        // check for the database existing, create if missing ??
        return conn;
    }

    private async Task CreateHome()
    {
        await Task.Delay(10);
    }

    public async Task<IEnumerable<GroceryItem>> GetListAsync(string homeId, CancellationToken cancel)
    {
        var sqlName = $"{homeId}_SelectAllCurrent";
        //var sql = await _map.GetSqlAsync(sqlName);
        var sql = _map.GetSql(sqlName);

        if (sql == null)
        {
            var builder = new SqlResourceBuilder(_map, "Grocery.SelectCurrent", homeId, _log);
            builder.Append("1 = 1;");

            sql = _map.SetSql(sqlName, builder.ToString());
        }

        await using var conn = GetConnection(); //homeId);
        return await conn.QueryAsync<GroceryItem>(new CommandDefinition(sql, cancellationToken: cancel)) ?? [];
    }
    public async Task<GroceryItem?> GetItemAsync(string homeId, string itemId, CancellationToken cancel)
    {
        var sqlName = $"{homeId}_SelectOneCurrent";
        //var sql = await _map.GetSqlAsync(sqlName);
        var sql = _map.GetSql(sqlName);

        if (sql == null)
        {
            var builder = new SqlResourceBuilder(_map, "Grocery.SelectCurrent", homeId, _log);
            builder.Append("`item_id` = @ItemId;");

            sql = _map.SetSql(sqlName, builder.ToString());
        }

        await using var conn = GetConnection(); //homeId);
        return await conn.QueryFirstOrDefaultAsync<GroceryItem>(new CommandDefinition(sql, new { ItemId = itemId, }, cancellationToken: cancel));
    }

    /// <summary>
    /// Adds/Updates current grocery list with specific item
    /// </summary>
    /// <param name="model">GroceryItem</param>
    /// <returns>GroceryItem</returns>
    public async Task<GroceryItem?> AddAsync(GroceryItem model, CancellationToken cancel)
    {
        var sqlName = $"{model.HomeId}_AddOrUpdateOne";
        //var sql = await _map.GetSqlAsync(sqlName);
        var sql = _map.GetSql(sqlName);

        if (sql == null)
        {
            var builder = new SqlResourceBuilder(_map, "Grocery.AddOrUpdate", model.HomeId, _log);
            var select = await _map.GetSqlAsync("Grocery.SelectCurrent");
            builder.Replace("{{SelectQuery}}", select);
            builder.ReplaceHome();

            sql = _map.SetSql(sqlName, builder.ToString());
        }

        await using var conn = GetConnection(); //model.HomeId);
        return await conn.QueryFirstOrDefaultAsync<GroceryItem>(sql, model);
    }

    /// <summary>
    /// Remove specific item from current grocery list
    /// </summary>
    /// <param name="model">GroceryItem</param>
    /// <returns>GroceryItem</returns>
    public async Task<GroceryItem?> DeleteAsync(GroceryItem model, CancellationToken cancel)
    {
        var sqlName = $"{model.HomeId}_DeleteOne";
        //var sql = await _map.GetSqlAsync(sqlName);
        var sql = _map.GetSql(sqlName);

        if (sql == null)
        {
            var builder = new SqlResourceBuilder(_map, "Grocery.DeleteCurrent", model.HomeId, _log);

            sql = _map.SetSql(sqlName, builder.ToString());
        }

        await using var conn = GetConnection(); //model.HomeId);
        var count = await conn.ExecuteAsync(new CommandDefinition(sql, new { Id = model.Id }, cancellationToken: cancel));
        return count == 0 ? null : model;
    }

    private SqlResourceBuilder GetCheckoutSql(string homeId)
    {
        var sql = new SqlResourceBuilder(_map, "Grocery.SelectCurrent", homeId, _log);
        sql.Append("`in_cart_time` IS NOT NULL"); // = @InCartTime
        return sql;
    }

    public async Task<IEnumerable<GroceryItem>> GetCheckoutAsync(string homeId, CancellationToken cancel)
    {
        var sqlName = $"{homeId}_GetCheckout";
        //var sql = await _map.GetSqlAsync(sqlName);
        var sql = _map.GetSql(sqlName);

        if (sql == null)
        {
            var builder = GetCheckoutSql(homeId).Append(';');

            sql = _map.SetSql(sqlName, builder.ToString());
        }

        await using var conn = GetConnection(); //homeId);
        return await conn.QueryAsync<GroceryItem>(new CommandDefinition(sql, cancellationToken: cancel)) ?? [];
    }
    public async Task<IEnumerable<GroceryItem>> CheckoutAsync(string homeId, List<string> checkoutItemIds, string? storeName, CancellationToken cancel)
    {
        // ISSUE: create parameter for each item in list?
        //        MAY HAVE TO CREATE THE ID PORTION OF THIS QUERY EVERY TIME!
        var sqlName = $"{homeId}_DoCheckout";
        //var sql = await _map.GetSqlAsync(sqlName);
        var sql = _map.GetSql(sqlName);

        if (sql == null)
        {
            var builder = GetCheckoutSql(homeId).Append("AND `item_id` IN(@Ids);");

            sql = _map.SetSql(sqlName, builder.ToString());
        }

        // build trip for checkoutItemIds, then delete from current
        await using var conn = GetConnection(); //homeId);
        var list = await conn.QueryAsync<GroceryItem>(new CommandDefinition(sql, new { Ids = checkoutItemIds, }, cancellationToken: cancel));

        return list ?? [];
    }

    public async Task<IEnumerable<GroceryTrip>> GetTripsAsync(string homeId, CancellationToken cancel)
    {
        var sqlName = $"{homeId}_SelectTrips";
        //var sql = await _map.GetSqlAsync(sqlName);
        var sql = _map.GetSql(sqlName);

        if (sql == null)
        {
            var builder = new SqlResourceBuilder(_map, "Grocery.SelectTrips", homeId, _log);

            sql = _map.SetSql(sqlName, builder.ToString());
        }

        // get a list of former trips
        await using var conn = GetConnection(); //homeId);
        return await conn.QueryAsync<GroceryTrip>(new CommandDefinition(sql, cancellationToken: cancel)) ?? [];
    }
}
