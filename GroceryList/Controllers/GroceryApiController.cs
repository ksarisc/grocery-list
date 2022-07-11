using GroceryList.Models;
using GroceryList.Models.Forms;
using GroceryList.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    [Authorize] //[Authorize(Roles = HomeRouteFilter.HasHome)]
    [ApiController]
    [Route("~/api/grocery")]
    public class GroceryApiController : ControllerBase
    {
        private readonly Services.IDataService dataSvc;
        private readonly Data.IUpdateCache cache;
        private readonly Data.IGroceryRepository repo;
        private readonly ILogger<GroceryController> logger;

        public GroceryApiController(Services.IDataService dataService, Data.IUpdateCache groceryCache,
            Data.IGroceryRepository groceryRepository, ILogger<GroceryController> groceryLogger)
        {
            dataSvc = dataService;
            cache = groceryCache;
            repo = groceryRepository;
            logger = groceryLogger;
            //lastUpdated = DateTime.MinValue; //groceryRepo.GetLastUpdated();
        }

        public async Task<bool> IsUpdated(string lastUpdatedTime)
        {
            // enforce UTC on server
            return await cache.IsUpdatedAsync("current", lastUpdatedTime);
        }

        [Route("getcurrent/{homeId}")]
        public async Task<ApiResult<IEnumerable<GroceryItem>>> GetCurrent(string homeId)
        {
            // display the current list
            // ?? should this log failures to invalid homeId's ??
            // ?? what should it return in that case ??
            try
            {
                var model = await repo.GetListAsync(homeId);
                return new ApiResult<IEnumerable<GroceryItem>>(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetCurrent (Home: {homeId}) ERR", homeId);
            }
            return new ApiResult<IEnumerable<GroceryItem>>("Unknown error happened");
        } // END GetCurrent

        [HttpPost("add")]
        public async Task<ApiResult<GroceryItemForm>> Add([FromBody] GroceryItemForm formModel)
        {
            if (!ModelState.IsValid)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var error in ModelState.Values.SelectMany(modelState => modelState.Errors))
                {
                    sb.Append(error.ErrorMessage);
                }
                //("Unable to add item: check details below");
                return new ApiResult<GroceryItemForm>(sb.ToString());
            }
            try
            {
                var model = formModel.ToModel();
                model.HomeId = formModel.HomeId;
                model.CreatedTime = DateTimeOffset.UtcNow;
                model.CreatedUser = User.Identity.Name; //GetUser();
                if (formModel.AddToCart && model.InCartTime == null)
                {
                    model.InCartTime = DateTimeOffset.UtcNow;
                    model.InCartUser = User.Identity.Name; //GetUser();
                }
                //PurchasedTime,PurchasedUser,
                var result = (await repo.AddAsync(model)).ToFormModel();
                await cache.UpdateAsync("current");
                return new ApiResult<GroceryItemForm>(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Add Error: {@formModel}", formModel);
            }
            return new ApiResult<GroceryItemForm>(formModel, "error in validation");
        } // END Add
    }
}
