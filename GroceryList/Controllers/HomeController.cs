using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    public class HomeController : Controller
    {
        private readonly Services.IDataService data;
        private readonly ILogger<HomeController> logger;

        public HomeController(Services.IDataService dataService, ILogger<HomeController> homeLogger)
        {
            data = dataService;
            logger = homeLogger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Models.Forms.HomeForm model)
        {
            string homeId = null;
            try
            {
                // after validation, setup the new home
                homeId = Guid.NewGuid().ToString();
                var home = new Models.Home
                {
                    Id = homeId,
                };
                home = await data.AddHomeAsync(home);

                return this.RedirectToGrocery(homeId);
            } catch (Exception ex)
            {
                logger.LogError(ex, "Home.Create ({0}) Error: {1}", homeId, model);
            }
            return View();
        }
    }
}
