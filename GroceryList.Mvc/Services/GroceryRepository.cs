using Dapper;
using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Config;
using GroceryList.Mvc.Models.Forms;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public interface IGroceryRepository : IDisposable
    {
        public Task<TripList> GetAsync();
        public Task SetAsync(TripList list);
        public Task<TripList> AddItemAsync(TripItemRequest itemRequest);
    }

    public class GroceryRepository : UserDataRepository<TripItem>, IGroceryRepository
    {
        // table version?
        private const string table = "list_active";
        private const string create = "CREATE TABLE list_active (name VARCHAR(255) NOT NULL, brand VARCHAR(255), requested_at DATETIME, created_at DATETIME NOT NULL, created_by VARCHAR(255) NOT NULL);";
        private const string select = "SELECT id Id, name Name, brand Brand, requested_at RequestedTime, created_at CreatedTime, created_by CreatedBy FROM list_active;";
        private const string insert = "INSERT INTO list_active (name, brand, requested_at, created_at, created_by) VALUES (@Name, @Brand, @RequestedTime, @CreatedTime, @CreatedBy);";
        //name = @Name, 
        private const string update = "UPDATE list_active SET brand = @Brand, requested_at = @RequestedTime WHERE id = @Id;";

        private static readonly UserDataConfig conf = new UserDataConfig
        {
            Table = table,
            Create = create,
            Select = select,
            Insert = insert,
            Update = update,
            Delete = null,
        };

        public GroceryRepository(IDataService dataService, AppUser appUser)
                            : base(dataService, appUser, conf)
        {
        }

        public async Task<TripList> GetAsync()
        {
            var list = new TripList(user.HomeId.Value);
            list.Items.AddRange(await GetAsync(null));
            return list;
        } // END GetListAsync

        public async Task SetAsync(TripList list)
        {
            // pull & compare OR backup & replace wholesale?
            foreach (var item in list.Items)
            {
                if (item.Id == 0)
                {
                    await InsertAsync(item);
                }
                else
                {
                    await UpdateAsync(item);
                }
            }
        } // END SetListAsync

        public async Task<TripList> AddItemAsync(TripItemRequest itemRequest)
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

            await InsertAsync(new TripItem
            {
                Name = itemRequest.ItemName,
                Brand = itemRequest.ItemBrand,
                RequestedTime = rqstTime,
                CreatedTime = DateTime.Now,
                CreatedBy = user.Email,
            });
            return await GetAsync();
        } // END AddItemAsync
    }
}
