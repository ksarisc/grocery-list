using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    [Route("")]
    [Route("~/[controller]")]
    public class HomeController : Controller
    {
        private readonly Services.IDataService _data;
        private readonly Serilog.ILogger _log;
        private readonly bool _allowAdd = false;
        private readonly string[]? _allowAddrs;

        public HomeController(Services.IDataService dataService, IOptions<Models.Config.GeneralConfig> options)
        {
            _log = Serilog.Log.Logger;
            _data = dataService;
            if (options != null && options.Value != null)
            {
                _allowAdd = options.Value.AllowHomeCreation;
                if (options.Value.AllowedIpAddresses?.Length > 0)
                {
                    _allowAddrs = options.Value.AllowedIpAddresses;
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

            if (!_allowAdd)
            {
                TempData["ErrorMessage"] = "You are NOT able to add homes currently.";
                return View(model);
            }
            var remote = HttpContext.GetRemoteIp();
            if (_allowAddrs != null && !Array.Exists(_allowAddrs, a => remote.Equals(a, StringComparison.Ordinal)))
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
                var result = await _data.AddHomeAsync(home);
                if (result == null)
                {
                    TempData["ErrorMessage"] = $"There was an error creating you home ({homeId}) {home.Title}.";
                    return View(model);
                }

                return this.RedirectToGrocery(result.Id, result.Title);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Home.Create ({0}) Error: {1}", homeId, model);
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
