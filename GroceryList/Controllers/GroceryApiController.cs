using GroceryList.Lib.Models;
using GroceryList.Models.Forms;
using GroceryList.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly GroceryList.Lib.IGroceryRepository _repo;
        private readonly ILogger<GroceryController> logger;

        public GroceryApiController(Services.IDataService dataService, Data.IUpdateCache groceryCache,
            GroceryList.Lib.IGroceryRepository groceryRepository, ILogger<GroceryController> groceryLogger)
        {
            dataSvc = dataService;
            cache = groceryCache;
            _repo = groceryRepository;
            logger = groceryLogger;
            //lastUpdated = DateTime.MinValue; //groceryRepo.GetLastUpdated();
        }

        public async Task<bool> IsUpdated(string lastUpdatedTime)
        {
            // enforce UTC on server
            return await cache.IsUpdatedAsync("current", lastUpdatedTime);
        }

        [Route("getcurrent/{homeId}")]
        public async Task<Models.ApiResult<IEnumerable<GroceryItem>>> GetCurrent(string homeId, CancellationToken cancel)
        {
            // display the current list
            // ?? should this log failures to invalid homeId's ??
            // ?? what should it return in that case ??
            try
            {
                var model = await _repo.GetListAsync(homeId, cancel);
                return new Models.ApiResult<IEnumerable<GroceryItem>>(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetCurrent (Home: {homeId}) ERR", homeId);
            }
            return new Models.ApiResult<IEnumerable<GroceryItem>>("Unknown error happened");
        } // END GetCurrent

        [HttpPost("add")]
        public async Task<Models.ApiResult<GroceryItemForm>> Add([FromBody] GroceryItemForm formModel, CancellationToken cancel)
        {
            var userName = User.Identity?.Name ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User NOT found");

            if (!ModelState.IsValid)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var error in ModelState.Values.SelectMany(modelState => modelState.Errors))
                {
                    sb.Append(error.ErrorMessage);
                }
                //("Unable to add item: check details below");
                return new Models.ApiResult<GroceryItemForm>(sb.ToString());
            }
            if (formModel.HomeId == null)
            {
                throw new ArgumentNullException(nameof(GroceryItemForm.HomeId));
            }
            try
            {
                var model = formModel.ToModel(formModel.HomeId);
                model.CreatedTime = DateTimeOffset.UtcNow;
                model.CreatedUser = userName; //GetUser();
                if (formModel.AddToCart && model.InCartTime == null)
                {
                    model.InCartTime = DateTimeOffset.UtcNow;
                    model.InCartUser = userName; //GetUser();
                }
                //PurchasedTime,PurchasedUser,
                var result = await _repo.AddAsync(model, cancel);
                if (result == null) return new Models.ApiResult<GroceryItemForm>($"Unable to add {model.Name} to list");

                await cache.UpdateAsync("current");
                return new Models.ApiResult<GroceryItemForm>(result.ToFormModel());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Add Error: {@formModel}", formModel);
            }
            return new Models.ApiResult<GroceryItemForm>(formModel, "error in validation");
        } // END Add
    }
}
