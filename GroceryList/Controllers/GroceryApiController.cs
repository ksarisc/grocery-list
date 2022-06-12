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
    //[Authorize(Roles = HomeRouteFilter.HasHome)]
    [ApiController]
    [Route("~/api/grocery")]
    public class GroceryApiController : ControllerBase
    {
        private readonly Services.IDataService dataSvc;
        private readonly Data.IGroceryRepository groceryRepo;
        private readonly ILogger<GroceryController> logger;

        private DateTime lastUpdated;

        public GroceryApiController(Services.IDataService dataService,
            Data.IGroceryRepository groceryRepository, ILogger<GroceryController> groceryLogger)
        {
            dataSvc = dataService;
            groceryRepo = groceryRepository;
            logger = groceryLogger;
            lastUpdated = DateTime.MinValue; //groceryRepo.GetLastUpdated();
        }

        public async Task<bool> IsUpdated(string lastUpdatedTime)
        {
            // enforce UTC on server
            await Task.Delay(100);
            return true;
        }

        [Route("getcurrent/{homeId}")]
        public async Task<Result<IEnumerable<GroceryItem>>> GetCurrent(string homeId)
        {
            // display the current list
            // ?? should this log failures to invalid homeId's ??
            // ?? what should it return in that case ??
            return await groceryRepo.GetListAsync(homeId);
        } // END GetCurrent

        [HttpPost("add")]
        public async Task<GroceryItemForm> Add([FromBody] GroceryItemForm formModel)
        {
            if (!ModelState.IsValid)
            {
                //("Unable to add item: check details below");
                throw new ArgumentException(ModelState.ErrorCount);
            }
            try
            {
                var model = formModel.ToModel();
                model.HomeId = formModel.HomeId;
                model.CreatedTime = DateTimeOffset.UtcNow;
                model.CreatedUser = GetUser();
                if (formModel.AddToCart)
                {
                    AddToCart(model);
                }
                //PurchasedTime,PurchasedUser,
                return (await groceryRepo.AddAsync(model)).ToFormModel();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Add Error: {@formModel}", formModel);
            }
            return formModel;
        } // END Add
    }
}
