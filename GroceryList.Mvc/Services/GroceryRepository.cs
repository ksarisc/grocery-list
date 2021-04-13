using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Forms;
using System;
using System.Collections.Generic;
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
        private readonly IDataService data;

        public GroceryRepository(IDataService dataService)
        {
            data = dataService;
        }

        public async Task<TripList> GetListAsync(AppUser user)
        {
            return await data.GetDataAsync<TripList>(user);
        }

        public async Task SetListAsync(AppUser user, TripList list)
        {
            await data.SetDataAsync<TripList>(user, list);
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

            var list = await GetListAsync(user);
            if (list == null)
            {
                list = new TripList();
                list.HomeId = user.HomeId.Value;
                list.Items = new List<TripItem>();
            }
            list.Items.Add(new TripItem
            {
                Name = itemRequest.ItemName,
                Brand = itemRequest.ItemBrand,
                RequestedTime = rqstTime,
                CreatedTime = DateTime.Now,
                CreatedBy = user.Email,
            });
            await SetListAsync(user, list);
            //return await GetListAsync(user);
            return list;
        } // END AddItem
    }
}
