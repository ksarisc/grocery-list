using GroceryList.Lib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Lib
{
    //public class GroceryRepository { }
    public interface IGroceryRepository
    {
        public Task<IEnumerable<GroceryItem>> GetListAsync(string homeId, CancellationToken cancel);
        public Task<GroceryItem?> GetItemAsync(string homeId, string itemId, CancellationToken cancel);

        /// <summary>
        /// Adds/Updates current grocery list with specific item
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public Task<GroceryItem?> AddAsync(GroceryItem model, CancellationToken cancel);
        /// <summary>
        /// Remove specific item from current grocery list
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public Task<GroceryItem?> DeleteAsync(GroceryItem model, CancellationToken cancel);

        public Task<IEnumerable<GroceryItem>> GetCheckoutAsync(string homeId, CancellationToken cancel);
        public Task<IEnumerable<GroceryItem>> CheckoutAsync(string homeId, List<string> checkoutItemIds, string? storeName, CancellationToken cancel);

        public Task<IEnumerable<GroceryTrip>> GetTripsAsync(string homeId, CancellationToken cancel);
    }
}
