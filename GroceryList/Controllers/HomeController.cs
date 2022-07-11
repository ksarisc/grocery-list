using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    [Route("")]
    [Route("~/[controller]")]
    public class HomeController : Controller
    {
        private readonly Services.IDataService data;
        private readonly ILogger<HomeController> logger;
        private readonly bool allowAdd = false;
        private readonly string[]? allowAddrs;

        public HomeController(Services.IDataService dataService, ILogger<HomeController> homeLogger,
                        IOptions<Models.Config.GeneralConfig> options)
        {
            data = dataService;
            logger = homeLogger;
            if (options != null && options.Value != null)
            {
                allowAdd = options.Value.AllowHomeCreation;
                if (options.Value.AllowedIpAddresses?.Length > 0)
                {
                    allowAddrs = options.Value.AllowedIpAddresses;
                }
            }
        }

        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create(Models.Forms.HomeForm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!allowAdd)
            {
                TempData["ErrorMessage"] = "You are NOT able to add homes currently.";
                return View(model);
            }
            var remote = HttpContext.GetRemoteIp();
            if (allowAddrs != null && !Array.Exists(allowAddrs, a => remote.Equals(a, StringComparison.Ordinal)))
            {
                TempData["ErrorMessage"] = "You are NOT able to add homes currently.";
                return View(model);
            }

            string? homeId = null;
            try
            {
                // after validation, setup the new home
                homeId = Utils.GetNewUuid();
                // if an error happens in save, should a new GUID be generated?
                var home = new Models.Home
                {
                    Id = homeId,
                    Title = model.Title,
                    CreatedBy = model.CreatedBy,
                    // creation details w/. meta
                    CreatedTime = DateTimeOffset.Now,
                    CreatedByMeta = $"IP:{remote}|UserAgent:{Request.Headers["User-Agent"]}",
                };
                home = await data.AddHomeAsync(home);

                return this.RedirectToGrocery(home.Id, home.Title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Home.Create ({0}) Error: {1}", homeId, model);
            }
            return View();
        } // END Create

        [Route("error")]
        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            });
        }
    }
}
