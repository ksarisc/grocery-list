using GroceryList.Models.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    [Route("{homeId}/[controller]")]
    public class GroceryController : Controller
    {
        private readonly Data.IGroceryRepository groceryRepo;
        private readonly ILogger<GroceryController> logger;
        public GroceryController(Data.IGroceryRepository groceryRepository, ILogger<GroceryController> groceryLogger)
        {
            groceryRepo = groceryRepository;
            logger = groceryLogger;
        }

        public async Task<IActionResult> Index(string homeId)
        {
            ViewData["HomeId"] = homeId;
            // display the current list
            var list = await groceryRepo.GetListAsync(homeId);
            return View(list);
        }

        // add/edit
        [HttpGet]
        public IActionResult Add(string homeId)
        {
            ViewData["HomeId"] = homeId;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromRoute] string homeId, [FromForm] GroceryItemForm formModel)
        {
            ViewData["HomeId"] = homeId;

            try
            {
                var model = new Models.GroceryItem
                {
                    HomeId = homeId,
                    Name = formModel.Name,
                    Brand = formModel.Brand,
                    Notes = formModel.Notes,
                    //Price = formModel.Price,
                    CreatedTime = DateTimeOffset.UtcNow,
                    CreatedUser = HttpContext.Connection.RemoteIpAddress.ToString(),
                    //InCartTime,InCartUser,
                    //PurchasedTime,PurchasedUser,
                };
                model = await groceryRepo.AddAsync(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Add Error: {0}", formModel);
                ViewData["ErrorMessage"] = "Unable to add the item";
            }
            return View(formModel);
        }
    }
}
