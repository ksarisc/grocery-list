using GroceryList.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public interface IGroceryRepository
    {
        public Task<List<GroceryItem>> GetListAsync(string homeId);
        public Task<GroceryItem> GetItemAsync(string homeId, string itemId);

        /// <summary>
        /// Adds/Updates current grocery list with specific item
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public Task<GroceryItem> AddAsync(GroceryItem model);
        /// <summary>
        /// Remove specific item from current grocery list
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public Task<GroceryItem> DeleteAsync(GroceryItem model);

        public Task<List<GroceryItem>> GetCheckoutAsync(string homeId);
        public Task<List<GroceryItem>> CheckoutAsync(string homeId, List<string> checkoutItemIds);

        public Task<List<GroceryTrip>> GetTripsAsync(string homeId);
    }
}
