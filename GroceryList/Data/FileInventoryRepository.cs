using GroceryList.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public class FileInventoryRepository : IInventoryRepository
    {
        public Task<IEnumerable<GroceryItem>> GetAllAsync(string homeId)
        {
            throw new NotImplementedException();
        }
        public Task<GroceryItem?> GetAsync(string homeId, string itemId)
        {
            throw new NotImplementedException();
        }

        public Task<GroceryItem?> AddAsync(GroceryItem model)
        {
            throw new NotImplementedException();
        }

        public Task<GroceryItem?> DeleteAsync(GroceryItem model)
        {
            throw new NotImplementedException();
        }
    }
}
