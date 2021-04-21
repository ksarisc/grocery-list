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
    [Authorize]
    public class GroceryController : Controller
    {
        private const string msgList = "Make changes/additions to your list";
        private readonly ILogger<HomeController> _logger;
        private readonly IGroceryRepository grocery;

        public GroceryController(ILogger<HomeController> logger,
                    IGroceryRepository groceryRepository)
        {
            _logger = logger;
            grocery = groceryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var message = msgList;
            try
            {
                var list = await grocery.GetAsync();
                ViewData["Message"] = message;
                return View(list);
            }
            catch (ArgumentNullException eArg)
            {
                _logger.LogError(eArg, $"GetList Arg Error: {Request.GetUser().GetContext()}: {message}");
                ViewData["Message"] = eArg.Message;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"GetList Error: {Request.GetUser().GetContext()}: {message}");
                ViewData["Message"] = "An unknown error has happened, please try again later";
            }
            return View();
        } // END List

        [HttpPost]
        public async Task<IActionResult> List(TripItemRequest model)
        {
            var message = msgList;
            try
            {
                var list = await grocery.AddItemAsync(model);
                ViewData["Message"] = message;
                return View(list);
            }
            // trap null arg exceptions?
            catch (ArgumentNullException eArg)
            {
                _logger.LogError(eArg, $"SetList Arg Error: {Request.GetUser().GetContext()}: {message}");
                ViewData["Message"] = eArg.Message;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"SetList Error: {Request.GetUser().GetContext()}: {message}");
                ViewData["Message"] = "An unknown error has happened, please try again later";
            }

            // redirect to List?
            return View();
        } // END List
    }
}
