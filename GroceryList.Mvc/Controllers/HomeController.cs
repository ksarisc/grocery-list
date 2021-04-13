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
        private const string msgIndex = "Make changes/additions to your list";
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
            var message = msgIndex;
            try
            {
                var list = await grocery.GetListAsync(user);
                ViewData["Message"] = message;
                return View(list);
            }
            catch (ArgumentNullException eArg)
            {
                _logger.LogError(eArg, $"GetList Arg Error: {user.GetContext()}: {message}");
                ViewData["Message"] = eArg.Message;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetList Error: {user.GetContext()}: {message}");
                ViewData["Message"] = "An unknown error has happened, please try again later";
            }
            return View();
        } // END Index

        [HttpPost]
        public async Task<IActionResult> Index(TripItemRequest model)
        {
            var user = Request.GetUser();
            var message = msgIndex;
            try
            {
                var list = await grocery.AddItem(user, model);
                ViewData["Message"] = message;
                return View(list);
            }
            // trap null arg exceptions?
            catch (ArgumentNullException eArg)
            {
                _logger.LogError(eArg, $"GetList Arg Error: {user.GetContext()}: {message}");
                ViewData["Message"] = eArg.Message;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetList Error: {user.GetContext()}: {message}");
                ViewData["Message"] = "An unknown error has happened, please try again later";
            }

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
