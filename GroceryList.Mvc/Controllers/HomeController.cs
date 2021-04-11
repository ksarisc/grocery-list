using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GroceryList.Mvc.Models;
using GroceryList.Mvc.Services;

namespace GroceryList.Mvc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService userSvc;

        public HomeController(ILogger<HomeController> logger,
                    IUserService userService)
        {
            _logger = logger;
            userSvc = userService;
        }

        public async Task<IActionResult> Index()
        {
            //await userSvc.
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //[AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
