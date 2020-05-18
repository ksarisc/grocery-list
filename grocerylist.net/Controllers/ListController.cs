using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using grocerylist.net.Models;
using grocerylist.net.Services;

namespace grocerylist.net.Controllers
{
    [Authorize(Policy = "User")] //Admin
    public class ListController : Controller
    {
        private readonly ILogger<ListController> logger;
        private readonly GroceryList list;

        public ListController(ILogger<ListController> logger, IConnectionService connects)
        {
            this.logger = logger;
            this.list = new GroceryList(connects.GetNew(),
                            HttpContext.User as HomeUser);
        }

        public IActionResult Index()
        {
            return View(list);
        }

        public async Task<IActionResult> Current()
        {
            return View(await list.GetItems());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Item(int id)
        {
            return View(await list.GetItemById(id));
        }

        [HttpPost]
        public async Task<IActionResult> Item(GroceryItem groceryItem)
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
