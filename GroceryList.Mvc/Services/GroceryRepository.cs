using Dapper;
using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Forms;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public interface IGroceryRepository
    {
        public Task<TripList> GetListAsync(AppUser user);
        public Task SetListAsync(AppUser user, TripList list);
        public Task<TripList> AddItem(AppUser user, TripItemRequest itemRequest);
    }

    public class GroceryRepository : IGroceryRepository
    {
        private const string create = "CREATE TABLE list_active (name, brand, requested_at, created_at, created_by);";
        private const string select = "SELECT id Id, name Name, brand Brand, requested_at RequestedTime, created_at CreatedTime, created_by CreatedBy FROM list_active;";
        private const string insert = "INSERT INTO list_active (name, brand, requested_at, created_at, created_by) VALUES (@Name, @Brand, @RequestedTime, @CreatedTime, @CreatedBy);";

        private readonly IDataService data;

        public GroceryRepository(IDataService dataService)
        {
            data = dataService;
        }

        private async Task<TripList> Get(AppUser user, DbConnection conn)
        {
            var list = new TripList(user.HomeId.Value);
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            list.Items.AddRange(await conn.QueryAsync<TripItem>(select));
            return list;
        }

        public async Task<TripList> GetListAsync(AppUser user)
        {
            using (var conn = data.GetConnection(user))
            {
                return await Get(user, conn);
            }
        } // END GetListAsync

        public async Task SetListAsync(AppUser user, TripList list)
        {
            await Task.Delay(10);
            // pull & compare OR backup & replace wholesale?
            throw new NotImplementedException();
        }

        public async Task<TripList> AddItem(AppUser user, TripItemRequest itemRequest)
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

                await conn.OpenAsync();
                await conn.ExecuteAsync(insert, new TripItem
                {
                    Name = itemRequest.ItemName,
                    Brand = itemRequest.ItemBrand,
                    RequestedTime = rqstTime,
                    CreatedTime = DateTime.Now,
                    CreatedBy = user.Email,
                });
                return await Get(user, conn);
            }
        } // END AddItem
    }
}
