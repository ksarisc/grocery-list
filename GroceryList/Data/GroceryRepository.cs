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
        /// <param name="model"></param>
        /// <returns></returns>
        public Task<GroceryItem> AddAsync(GroceryItem model);
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

        /// <summary>
        /// Adds/Updates current grocery list with specific item
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<GroceryItem> AddAsync(GroceryItem model)
        {
            if (string.IsNullOrWhiteSpace(model?.HomeId)) return null;
            // validate item

            //model.Id = ""

            var list = await GetListAsync(model.HomeId);
            var found = false;
            if (list == null) list = new List<GroceryItem>();

            for (int i = 0; i != list.Count; i++)
            {
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
        }
    }
}
