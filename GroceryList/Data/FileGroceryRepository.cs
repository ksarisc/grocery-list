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
    public class FileGroceryRepository : IGroceryRepository
    {
        private const string currentFile = "current_list";

        private readonly IDataService fileService;
        //private readonly IHttpContextAccessor context;
        public FileGroceryRepository(IDataService dataFileService) //, IHttpContextAccessor contextAccessor)
        {
            fileService = dataFileService;
            //context = contextAccessor;
        }

        public async Task<IEnumerable<GroceryItem>> GetListAsync(string homeId)
        {
            // get Home ID & correct current data file
            var list = await fileService.GetAsync<List<GroceryItem>>(homeId, currentFile);
            if (list != null) return list;
            return Array.Empty<GroceryItem>();
        }
        public async Task<GroceryItem?> GetItemAsync(string homeId, string itemId)
        {
            var list = await fileService.GetAsync<List<GroceryItem>>(homeId, currentFile);
            if (list == null) return null;
            if (!list.Any()) return null;

            return list.FirstOrDefault(g => g.Id.Equals(itemId, StringComparison.Ordinal));
        }

        public async Task<GroceryItem?> AddAsync(GroceryItem model)
        {
            if (string.IsNullOrWhiteSpace(model?.HomeId)) return null;
            // validate item

            // clean fields in model
            //model.Name = model.Name.Trim();

            var list = (await GetListAsync(model.HomeId)).AsList();

            var found = false;
            for (int i = 0; i != list.Count; i++)
            {
                var item = list[i];
                // ?? check by name & id ??
                if (string.Equals(item.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    model.Id = !string.IsNullOrWhiteSpace(item.Id) ?
                        item.Id : Utils.GetNewUuid();
                    model.CreatedTime = item.CreatedTime;
                    model.CreatedUser = item.CreatedUser;
                    //model.InCartTime = item.InCartTime;
                    //model.InCartUser = item.InCartUser;
                    //model.PurchasedTime = item.PurchasedTime;
                    //model.PurchasedUser = item.PurchasedUser;
                    list[i] = model;
                    break;
                }
            }
            if (!found)
            {
                // initialize
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    model.Id = Utils.GetNewUuid();
                }
                list.Add(model);
            }

            await fileService.SetAsync(model.HomeId, currentFile, list);
            return model;
        } // END AddAsync

        public async Task<GroceryItem?> DeleteAsync(GroceryItem model)
        {
            // ?? throw ??
            if (string.IsNullOrWhiteSpace(model?.Id)) return null;
            if (string.IsNullOrWhiteSpace(model?.HomeId)) return null;

            var list = (await GetListAsync(model.HomeId)).AsList();

            if (list.Count == 0) throw new ArgumentOutOfRangeException("No List Found");

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

        public async Task<IEnumerable<GroceryItem>> GetCheckoutAsync(string homeId)
        {
            // get the list of items in cart
            var list = await GetListAsync(homeId);
            return list.Where(g => g.InCartTime != null).ToList();
        }
        //public async Task<IEnumerable<GroceryItem>> CheckoutAsync(string homeId){
        //    // get the list of items in cart
        //    var list = await GetListAsync(homeId);
        //    var origList = list.ToArray();
        //    var inCart = list.Where(g => g.InCartTime != null).ToList();
        //    inCart.ForEach(g =>{
        //        list.RemoveAll(gl => gl.Id.Equals(g.Id, StringComparison.Ordinal));
        //    });

        //    // save the remaining list
        //    await fileService.SetAsync(homeId, currentFile, list);

        //    // save the trip
        //    try{
        //        var tripRqst = new DataRequest{
        //            HomeId = homeId,
        //            StoreName = currentFile,
        //            ActionName = "trip",
        //        };
        //        await fileService.SetAsync(tripRqst, inCart);
        //    }catch (Exception){
        //        // ?? rollback on error here ??
        //        await fileService.SetAsync(homeId, currentFile, origList);
        //        throw;
        //    }

        //    // return the trip items
        //    return inCart;
        //} // END CheckoutAsync
        public async Task<IEnumerable<GroceryItem>> CheckoutAsync(string homeId, List<string> checkoutItemIds, string? storeName)
        {
            // get the list of items in cart
            var list = (await GetListAsync(homeId)).AsList();
            var origList = list.ToArray();
            var inCart = new List<GroceryItem>();
            checkoutItemIds.ForEach(g =>
            {
                // add item from current to in cart
                var index = list.FindIndex(gl => gl.Id.Equals(g, StringComparison.Ordinal));
                inCart.Add(list[index]);
                list.RemoveAt(index);
            });

            // save the remaining list
            await fileService.SetAsync(homeId, currentFile, list);

            // save the trip
            try
            {
                if (storeName == null) storeName = "NA";
                var tripData = new GroceryTripData
                {
                    StoreName = storeName,
                    Items = inCart.ToArray(),
                };

                var tripRqst = new DataRequest
                {
                    HomeId = homeId,
                    StoreName = currentFile, //$"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{storeName}",
                    ActionName = "trip",
                };
                await fileService.SetAsync(tripRqst, tripData);
            }
            catch (Exception)
            {
                // ?? rollback on error here ??
                await fileService.SetAsync(homeId, currentFile, origList);
                throw;
            }

            // return the trip items
            return inCart;
        } // END CheckoutAsync

        public async Task<IEnumerable<GroceryTrip>> GetTripsAsync(string homeId)
        {
            var tripRqst = new DataRequest{
                    HomeId = homeId,
                    StoreName = currentFile,
                    ActionName = "trip",
                };
            // list all previous trips (range?)
            var files = await fileService.ListAsync(tripRqst);
            var list = new List<GroceryTrip>();

            // 

            return list;
        }
    }
}
