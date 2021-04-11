using GroceryList.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryList.Mvc.Services
{
    public interface IGroceryRepository
    {
        public Task<TripList> GetListAsync(AppUser user);
        public Task SetListAsync(AppUser user, TripList list);
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
    }
}
