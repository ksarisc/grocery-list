using GroceryList.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public interface IInventoryRepository
    {
        public Task<IEnumerable<GroceryItem>> GetAllAsync(string homeId);
        public Task<GroceryItem?> GetAsync(string homeId, string itemId);

        /// <summary>
        /// Adds/Updates current inventory list with specific item
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public Task<GroceryItem?> AddAsync(GroceryItem model);
        /// <summary>
        /// Remove specific item from current inventory list
        /// </summary>
        /// <param name="model">GroceryItem</param>
        /// <returns>GroceryItem</returns>
        public Task<GroceryItem?> DeleteAsync(GroceryItem model);
    }
}
