using Dapper;
using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Forms;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public interface IGroceryRepository
    {
        public Task<TripList> GetAsync(AppUser user);
        public Task SetAsync(AppUser user, TripList list);
        public Task<TripList> AddItemAsync(AppUser user, TripItemRequest itemRequest);
    }

    public class GroceryRepository : IGroceryRepository
    {
        // table version?
        private const string table = "list_active";
        private const string create = "CREATE TABLE list_active (name VARCHAR(255) NOT NULL, brand VARCHAR(255), requested_at DATETIME, created_at VARCHAR(255) NOT NULL, created_by DATETIME NOT NULL);";
        private const string select = "SELECT id Id, name Name, brand Brand, requested_at RequestedTime, created_at CreatedTime, created_by CreatedBy FROM list_active;";
        private const string insert = "INSERT INTO list_active (name, brand, requested_at, created_at, created_by) VALUES (@Name, @Brand, @RequestedTime, @CreatedTime, @CreatedBy);";
        //name = @Name, 
        private const string update = "UPDATE list_active SET brand = @Brand, requested_at = @RequestedTime WHERE id = @Id;";

        private readonly IDataService data;

        public GroceryRepository(IDataService dataService)
        {
            data = dataService;
        }

        // public async Task CreateAsync(AppUser user){
        //     using (var conn = data.GetConnection(user)){
        //         await conn.OpenAsync();
        //         await conn.ExecuteAsync(create);
        //     }
        // } // END CreateAsync

        private async Task<TripList> Get(AppUser user, DbConnection conn)
        {
            var list = new TripList(user.HomeId.Value);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            await data.TableExists(conn, table, create);
            list.Items.AddRange(await conn.QueryAsync<TripItem>(select));
            return list;
        } // END Get

        private static async Task<int> Insert(DbConnection conn, TripItem item)
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            return await conn.ExecuteAsync(insert, item);
        } // END Insert

        private static async Task<int> Update(DbConnection conn, TripItem item)
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            return await conn.ExecuteAsync(update, item);
        } // END Update

        // DELETE

        public async Task<TripList> GetAsync(AppUser user)
        {
            using (var conn = data.GetConnection(user))
            {
                return await Get(user, conn);
            }
        } // END GetListAsync

        public async Task SetAsync(AppUser user, TripList list)
        {
            // pull & compare OR backup & replace wholesale?
            using (var conn = data.GetConnection(user))
            {
                foreach (var item in list.Items)
                {
                    if (item.Id == 0)
                    {
                        await Insert(conn, item);
                    }
                    else
                    {
                        await Update(conn, item);
                    }
                }
            }
        } // END SetListAsync

        public async Task<TripList> AddItemAsync(AppUser user, TripItemRequest itemRequest)
        {
            if (itemRequest == null)
            {
                throw new ArgumentNullException("A Grocery Trip Item is REQUIRED!");
            }
            if (String.IsNullOrWhiteSpace(itemRequest.ItemName))
            {
                throw new ArgumentNullException("Item Name REQUIRED!");
            }
            if (!user.HomeId.HasValue)
            {
                throw new ArgumentNullException("User is NOT valid!");
            }
            // fix empty brand
            if (String.IsNullOrWhiteSpace(itemRequest.ItemBrand))
            {
                itemRequest.ItemBrand = null;
            }
            // handle non-date time
            DateTime? rqstTime = null;

            // if (list == null)
            //     throw new Exception("User/Home NOT Valid");

            using (var conn = data.GetConnection(user))
            {
                await Insert(conn, new TripItem
                {
                    Name = itemRequest.ItemName,
                    Brand = itemRequest.ItemBrand,
                    RequestedTime = rqstTime,
                    CreatedTime = DateTime.Now,
                    CreatedBy = user.Email,
                });
                return await Get(user, conn);
            }
        } // END AddItemAsync
    }
}
