using GroceryList.Models.Forms;
using GroceryList.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    // NO DATA CHANGE WITH HttpGet ONLY WITH HttpPost/Put/Patch/Delete

    //[Authorize(Roles = HomeRouteFilter.HasHome)]
    [Route(HomeRouteFilter.Route)]
    public class GroceryController : Controller
    {
        private readonly Data.IGroceryRepository groceryRepo;
        private readonly ILogger<GroceryController> logger;
        public GroceryController(Data.IGroceryRepository groceryRepository, ILogger<GroceryController> groceryLogger)
        {
            groceryRepo = groceryRepository;
            logger = groceryLogger;
        }

        private string GetUser()
        {
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }

        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(string homeId)
        {
            this.SetHomeId(homeId);
            // display the current list
            var list = await groceryRepo.GetListAsync(homeId);
            return View(list);
        }

        // add/edit
        [HttpGet("add")]
        public IActionResult Add(string homeId)
        {
            this.SetHomeId(homeId);
            return View();
        }
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromRoute] string homeId, [FromForm] GroceryItemForm formModel)
        {
            this.SetHomeId(homeId);

            if (ModelState.IsValid)
            {
                try
                {
                    var model = new Models.GroceryItem
                    {
                        HomeId = homeId,
                        Name = formModel.Name,
                        Brand = formModel.Brand,
                        Notes = formModel.Notes,
                        //Price = formModel.Price,
                        CreatedTime = DateTimeOffset.UtcNow,
                        CreatedUser = GetUser(),
                        //InCartTime,InCartUser,
                        //PurchasedTime,PurchasedUser,
                    };
                    model = await groceryRepo.AddAsync(model);
                    TempData["InfoMessage"] = $"{model.Name} added to list";
                    TempData["ErrorMessage"] = null;
                    return this.RedirectToGrocery();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Add Error ({0}): {0}", homeId, formModel);
                    ViewData["ErrorMessage"] = "Unable to add the item";
                }
            }
            return View(formModel);
        } // END Add

        [HttpGet("edit/{itemId}")]
        public async Task<IActionResult> Edit([FromRoute] string homeId, [FromRoute] string itemId)
        {
            this.SetHomeId(homeId);
            if (string.IsNullOrWhiteSpace(itemId))
            {
                TempData["ErrorMessage"] = $"No item specified";
                return this.RedirectToGrocery(homeId);
            }
            //TempData["ErrorMessage"] = null; //$"Unable to edit item ({itemId})";
            //return this.RedirectToGrocery(homeId);
            var model = await groceryRepo.GetItemAsync(homeId, itemId);
            return View(model);
        }
        [HttpPost("edit/{itemId}")]
        public async Task<IActionResult> Edit([FromRoute] string homeId, [FromRoute] string itemId,
            [FromForm] Models.GroceryItem model)
        {
            this.SetHomeId(homeId);
            if (string.IsNullOrWhiteSpace(itemId) || model == null || string.IsNullOrWhiteSpace(model.Id))
            {
                TempData["ErrorMessage"] = $"No item specified";
                return this.RedirectToGrocery(homeId);
            }
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = $"Unable to edit item ({itemId})";
                return View(model);
            }
            try
            {
                model = await groceryRepo.AddAsync(model);
                TempData["InfoMessage"] = $"{model.Name} edited";
                TempData["ErrorMessage"] = null;
                return this.RedirectToGrocery(homeId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Edit Error ({0}): {0}", homeId, model);
                ViewData["ErrorMessage"] = "Unable to edit the item";
            }
            return View(model);
        } // END Edit

        [HttpGet("delete/{itemId}")]
        public async Task<IActionResult> Delete([FromRoute] string homeId, [FromRoute] string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                TempData["ErrorMessage"] = "No item specified";
                return this.RedirectToGrocery(homeId);
            }

            try
            {
                var model = await groceryRepo.GetItemAsync(homeId, itemId);
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Delete.Get Error: {0}|{1}", homeId, itemId);
                TempData["ErrorMessage"] = $"Unable to remove the item: {itemId}";
            }
            return this.RedirectToGrocery(homeId);
        } // END Delete
        // Post is confirmation
        [HttpPost("delete/{itemId}")]
        public async Task<IActionResult> Delete([FromRoute] string homeId,
            [FromRoute] string itemId, [FromForm] Models.GroceryItem model)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                TempData["ErrorMessage"] = "No item specified";
                return this.RedirectToGrocery(homeId);
            }
            if (!itemId.Equals(model.Id, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "Mismatch with item specified";
                return this.RedirectToGrocery(homeId);
            }

            try
            {
                //TempData["ErrorMessage"] = $"Unable to delete item ({itemId}) from list";
                model = await groceryRepo.DeleteAsync(model);
                TempData["InfoMessage"] = $"Item ({model.Name}) removed from list";
                TempData["ErrorMessage"] = null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Delete Error: {0}|{1}", homeId, itemId);
                ViewData["ErrorMessage"] = "Unable to remove the item";
            }

            return this.RedirectToGrocery(homeId);
        } // END Delete

        [Route("tocart/{itemId}")]
        public async Task<IActionResult> ToCart([FromRoute] string homeId, string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                TempData["ErrorMessage"] = $"No item specified";
                return this.RedirectToGrocery(homeId);
            }
            //TempData["ErrorMessage"] = $"Unable to move item ({itemId}) to cart";

            try
            {
                var model = await groceryRepo.GetItemAsync(homeId, itemId);
                if (model == null)
                {
                    TempData["ErrorMessage"] = $"Missing grocery item ({itemId})";
                    return this.RedirectToGrocery(homeId);
                }
                model.InCartTime = DateTimeOffset.UtcNow;
                model.InCartUser = GetUser();
                model = await groceryRepo.AddAsync(model);
                TempData["InfoMessage"] = $"{model.Name} added to cart";
                TempData["ErrorMessage"] = null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Delete Error: {0}|{1}", homeId, itemId);
                ViewData["ErrorMessage"] = "Unable to remove the item";
            }

            return this.RedirectToGrocery(homeId);
        }

        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout([FromRoute] string homeId)
        {
            //TempData["ErrorMessage"] = $"Unable to checkout cart";

            try
            {
                // checkout all items currently in cart
                var list = await groceryRepo.GetCheckoutAsync(homeId);

                if (list == null || !list.Any())
                {
                    ViewData["ErrorMessage"] = "There are no items in your cart";
                    return this.RedirectToGrocery(homeId);
                }

                return View(new CheckoutForm
                {
                    HomeId = homeId,
                    Items = list,
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Checkout.Get Error: {0}", homeId);
                ViewData["ErrorMessage"] = "Checkout failed";
            }
            return this.RedirectToGrocery(homeId);
        } // END Checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromRoute] string homeId, CheckoutForm model)
        {
            List<Models.GroceryItem> list = null;

            try
            {
                // checkout all items currently in cart
                list = await groceryRepo.CheckoutAsync(homeId, model.ItemIds);
                TempData["InfoMessage"] = $"{list.Count} items marked as purchased";
                TempData["ErrorMessage"] = null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Checkout Error: {0} (InCart: {1})", homeId, list);
                ViewData["ErrorMessage"] = "Checkout failed";
            }
            return this.RedirectToGrocery(homeId);
        } // END Checkout
    }
}
