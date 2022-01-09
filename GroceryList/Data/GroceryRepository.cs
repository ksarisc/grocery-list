using GroceryList.Models;
using GroceryList.Models.Forms;
using GroceryList.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Data
{
    public interface IGroceryRepository
    {
        public Task<List<GroceryItem>> GetListAsync(string homeId);

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
    }

    public class GroceryRepository : IGroceryRepository
    {
        private const string currentFile = "current_list";

        private readonly IDataService fileService;
        //private readonly IHttpContextAccessor context;
        public GroceryRepository(IDataService dataFileService) //, IHttpContextAccessor contextAccessor)
        {
            fileService = dataFileService;
            //context = contextAccessor;
        }

        public async Task<List<GroceryItem>> GetListAsync(string homeId)
        {
            // get Home ID & correct current data file
            return await fileService.GetAsync<List<GroceryItem>>(homeId, currentFile);
        }

        public async Task<GroceryItem> AddAsync(GroceryItem model)
        {
            if (string.IsNullOrWhiteSpace(model?.HomeId)) return null;
            // validate item

            // initialize
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                model.Id = Guid.NewGuid().ToString();
            }

            var list = await GetListAsync(model.HomeId);
            var found = false;
            if (list == null) list = new List<GroceryItem>();

            for (int i = 0; i != list.Count; i++)
            {
                // ?? check by name & id ??
                if (list[i].Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    list[i] = model;
                    break;
                }
            }
            if (!found)
            {
                list.Add(model);
            }

            await fileService.SetAsync(model.HomeId, currentFile, list);
            return model;
        } // END AddAsync

        public async Task<GroceryItem> DeleteAsync(GroceryItem model)
        {
            // ?? throw ??
            if (string.IsNullOrWhiteSpace(model?.Id)) return null;
            if (string.IsNullOrWhiteSpace(model?.HomeId)) return null;

            var list = await GetListAsync(model.HomeId);

            if (list == null) throw new ArgumentOutOfRangeException("No List Found");

            list.RemoveAll(g =>
            {
                if (g.Id.Equals(model.Id, StringComparison.Ordinal))
                {
                    model = g;
                    return true;
                }
                return false;
            });

            await fileService.SetAsync(model.HomeId, currentFile, list);
            return model;
        } // END DeleteAsync
    }
}
