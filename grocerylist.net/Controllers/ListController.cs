using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using grocerylist.net.Models.Grocery;
using grocerylist.net.Services;

namespace grocerylist.net.Controllers
{
    [Authorize(Policy = "User")] //Admin
    public class ListController : Controller
    {
        private readonly ILogger<ListController> logger;
        private readonly IGroceriesRepository groceries;

        public ListController(ILogger<ListController> logger, IGroceriesRepository groceries)
        {
            this.logger = logger;
            this.groceries = groceries;
        }

        public async Task<IActionResult> Index()
        {
            return View(await groceries.GetCurrentItemsAsync(User));
        }

        public async Task<IActionResult> Current()
        {
            return View(await groceries.GetCurrentItemsAsync(User));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Item(int id)
        {
            return View(await groceries.GetItemByIdAsync(User, id));
        }

        [HttpPost]
        public async Task<IActionResult> Item(Item item)
        {
            try {
                var item = await list.EditItem(groceryItem);
                ViewBag["Message"] = "Grocery list updated!";
                return View(item);
            } catch (Exception eSet) {
                logger.LogError(eSet, "Grocery Item Update Error", groceryItem);
                ViewBag["Message"] = "Grocery list updated!";
            }
            return View(groceryItem);
        } // END Item
    }
}
