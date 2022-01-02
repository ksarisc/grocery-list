using GroceryList.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    public class HomeController : Controller
    {
        private readonly Data.IHomeRepository home;
        private readonly ILogger<HomeController> logger;

        public HomeController(Data.IHomeRepository homeRepository, ILogger<HomeController> homeLogger)
        {
            home = homeRepository;
            logger = homeLogger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index([FromForm]string textHomeId)
        {
            ViewData["Message"] = "Not implemented yet!";

            if (!(await home.ExistsAsync(textHomeId)))
            {
                ViewData["Message"] = $"Home ({textHomeId}) NOT Found";
            }
            await home.AddApproveeAsync(User, textHomeId);
                //.AddAsync(model);

            return View();
        }

        public IActionResult Approve()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
