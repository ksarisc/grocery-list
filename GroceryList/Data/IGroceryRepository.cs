using GroceryList.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public interface IGroceryRepository
    {
        public Task<IEnumerable<GroceryItem>> GetListAsync(string homeId);
        public Task<GroceryItem?> GetItemAsync(string homeId, string itemId);

        /// <summary>
        /// Adds/Updates current grocery list with specific item
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public Task<GroceryItem?> AddAsync(GroceryItem model);
        /// <summary>
        /// Remove specific item from current grocery list
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public Task<GroceryItem?> DeleteAsync(GroceryItem model);

        public Task<IEnumerable<GroceryItem>> GetCheckoutAsync(string homeId);
        public Task<IEnumerable<GroceryItem>> CheckoutAsync(string homeId, List<string> checkoutItemIds, string? storeName);

        public Task<IEnumerable<GroceryTrip>> GetTripsAsync(string homeId);
    }
}
