using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    [Route("{homeId}/[controller]")]
    public class GroceryController : Controller
    {
        private readonly Data.IGroceryRepository groceryRepo;
        public GroceryController(Data.IGroceryRepository groceryRepository)
        {
            groceryRepo = groceryRepository;
        }

        public async Task<IActionResult> Index(string homeId)
        {
            // display the current list
            var list = await groceryRepo.GetListAsync(homeId);
            return View(list);
        }

        // add/edit
    }
}
