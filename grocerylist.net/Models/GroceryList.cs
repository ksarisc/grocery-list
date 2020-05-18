using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;

namespace grocerylist.net.Models
{
    public class GroceryList : IDisposable
    {
        private static readonly string baseQuery = "SELECT * FROM grocerylist WHERE home_id = @HomeId";
        private static readonly string idQuery = " AND id = @Id";
        private static readonly string currentQuery = " AND purchased_at IS NULL";
        private static readonly string insert = $"INSERT INTO grocerylist (home_id, name, brand, created_at, requested_at, requested_by) VALUES (@HomeId, @Name, @Brand, @CreatedAt, @RequestedAt, @UserId); {baseQuery} AND id = LAST_INSERT_ID();";
        private static readonly string update = $"UPDATE grocerylist SET name = @Name, brand = @Brand, requested_at = @RequestedAt, requested_by = @UserId WHERE id = @Id; {baseQuery}{idQuery};";
        private readonly DbConnection conn;
        private readonly HomeUser user;

        public GroceryList(DbConnection connection, HomeUser user)
        {
            conn = connection;
            this.user = user;
        }

        public async Task<IEnumerable<GroceryItem>> GetItems(bool getCurrent = true)
        {
            var parm = new {
                //PurchasedAt = DBNull.Value
                HomeId = user.HomeId
            };
            //" AND purchased_at BETWEEN "
            var query = getCurrent ?
                        baseQuery + currentQuery :
                        baseQuery;
            if (conn.State != ConnectionState.Open) {
                await conn.OpenAsync();
            }
            //config.GroceryListQuery,
            return await conn.QueryAsync<GroceryItem>(query, parm);
        } // END GetItems

        public async Task<GroceryItem> GetItemById(int id)
        {
            var parm = new {
                HomeId = user.HomeId,
                Id = id
            };
            var query = baseQuery + idQuery;
            if (conn.State != ConnectionState.Open) {
                await conn.OpenAsync();
            }
            return (await conn.QueryAsync<GroceryItem>(query, parm))
                .FirstOrDefault();
        } // END GetItemById

        public async Task<GroceryItem> EditItem(GroceryItem item)
        {
            var parm = new {
                HomeId = user.HomeId,
                Name = item.Name,
                Brand = item.Brand,
                CreatedAt = item.CreatedAt,
                RequestedAt = item.RequestedAt,
                UserId = user.Id,
                Id = item.Id
            };
            var query = item.Id == 0 ? insert : update;
            if (conn.State != ConnectionState.Open) {
                await conn.OpenAsync();
            }
            //await conn.ExecuteAsync(query, parm);
            //return await GetItemById(id);
            return (await conn.QueryAsync<GroceryItem>(query, parm))
                .FirstOrDefault();
        } // END EditItem

        #region clean-up
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) {
                return;
            }
            conn.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~GroceryList()
        {
            Dispose(false);
        }
        #endregion clean-up
    }
}
