using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using grocerylist.net.Models.Grocery;
using grocerylist.net.Models.Security;
using grocerylist.net.Services.Grocery;

namespace grocerylist.net.Controllers
{
    //[Authorize(Policy = "User")] //Admin
    [Authorize]
    public class ListController : Controller
    {
        private readonly ILogger<ListController> logger;
        private readonly IGroceriesRepository repo;

        public ListController(ILogger<ListController> logger, IGroceriesRepository groceries)
        {
            this.logger = logger;
            this.repo = groceries;
        }

        public async Task<IActionResult> Index()
        {
            return View(await repo.GetCurrentItemsAsync(User.GetHomeUser()));
        }

        public async Task<IActionResult> Current()
        {
            return View(await repo.GetCurrentItemsAsync(User.GetHomeUser()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Item(uint id)
        {
            return View(await repo.GetItemByIdAsync(User.GetHomeUser(), id));
        }

        [HttpPost]
        public async Task<IActionResult> Item(Item item)
        {
            try {
                var result = await repo.SaveAsync(User.GetHomeUser(), item);
                ViewBag["Message"] = "Grocery list updated!";
                return View(result);
            } catch (Exception eSet) {
                logger.LogError(eSet, "Grocery Item Update Error", item);
                ViewBag["Message"] = "Grocery list updated!";
            }
            return View(item);
        } // END Item
    }
}
