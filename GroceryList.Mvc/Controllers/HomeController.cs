using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Forms;
using GroceryList.Mvc.Services;

namespace GroceryList.Mvc.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGroceryRepository grocery;

        public HomeController(ILogger<HomeController> logger,
                    IGroceryRepository groceryRepository)
        {
            _logger = logger;
            grocery = groceryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = Request.GetUser();
            try
            {
                var list = await grocery.GetListAsync(user);
                return View(list);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetList Error: {user.GetContext()}");
            }
            return View();
        } // END Index

        [HttpPost]
        public async Task<IActionResult> Index(TripItemRequest model)
        {
            var user = Request.GetUser();
            // set message

            // redirect to Index?

            return View();
        } // END Index

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
