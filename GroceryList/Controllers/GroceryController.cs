using GroceryList.Lib.Models;
using GroceryList.Models.Forms;
using GroceryList.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryList.Controllers
{
    // NO DATA CHANGE WITH HttpGet ONLY WITH HttpPost/Put/Patch/Delete
    // Groceries should probably have an API controller to make testing easier

    //[Authorize(Roles = HomeRouteFilter.HasHome)]
    [Route(HomeRouteFilter.Route)]
    public class GroceryController : Controller
    {
        private readonly IDataService _data;
        private readonly Lib.IGroceryRepository _repo;
        private readonly ILogger<GroceryController> _log;

        private string homeId = string.Empty;
        [FromRoute]
        public string HomeId
        {
            get { return homeId; }
            set { homeId = value; }
        }

        public GroceryController(IDataService dataService, Lib.IGroceryRepository groceryRepository, ILogger<GroceryController> groceryLogger)
        {
            _data = dataService;
            _repo = groceryRepository;
            _log = groceryLogger;
        }

        private string GetUser()
        {
            return HttpContext.GetRemoteIp();
        }

        private async Task SetHomeAsync()
        {
            // ?? throw if homeId is null, empty, or whitespace ??
            var localId = HttpContext.GetHomeId();
            if (homeId.Equals(localId, StringComparison.Ordinal)) return;
            // should each page check that the cookie matches the route? sounds like auth?
            try
            {
                var home = await _data.GetHomeAsync(homeId);
                if (home != null) HttpContext.SetHome(home.Id, home.Title);
                else HttpContext.SetHome(homeId, string.Empty);
            }
            catch (Exception ex)
            {
                HttpContext.SetHome(homeId, string.Empty);
                _log.LogError(ex, "Grocery.SetHome ({homeId}) Error", homeId);
            }
        }

        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index(CancellationToken cancel)
        {
            await SetHomeAsync();

            // display the current list
            var list = await _repo.GetListAsync(homeId, cancel);
            return View(list);
        }

        private Lib.Models.GroceryItem AddToCart(GroceryItem model)
        {
            if (model.InCartTime == null)
            {
                model.InCartTime = DateTimeOffset.UtcNow;
                model.InCartUser = GetUser();
            }
            return model;
        }

        // add/edit
        [HttpGet("add")]
        public IActionResult Add()
        {
            //this.SetHomeId(homeId);
            return View();
        }
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm] GroceryItemForm formModel, CancellationToken cancel)
        {
            var user = GetUser();
            //formModel.Id = "temp";
            formModel.CreatedUser = user;
            ModelState.Remove(nameof(GroceryItemForm.Id));
            ModelState.Remove(nameof(GroceryItemForm.CreatedUser));
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = $"Unable to add item: check details below";
                return View(formModel);
            }
            try
            {
                var model = formModel.ToModel(homeId);
                model.CreatedTime = DateTimeOffset.UtcNow;
                model.CreatedUser = user;
                if (formModel.AddToCart)
                {
                    AddToCart(model);
                }
                //PurchasedTime,PurchasedUser,
                model = await _repo.AddAsync(model, cancel);
                TempData["InfoMessage"] = $"{model?.Name} added";
                TempData["ErrorMessage"] = null;
                return this.RedirectToGrocery();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Add Error ({0}): {0}", homeId, formModel);
                ViewData["ErrorMessage"] = "Unable to add the item";
            }
            return View(formModel);
        } // END Add

        [HttpGet("edit/{itemId}")]
        public async Task<IActionResult> Edit([FromRoute] string itemId, CancellationToken cancel)
        {
            //this.SetHomeId(homeId);
            if (string.IsNullOrWhiteSpace(itemId))
            {
                TempData["ErrorMessage"] = $"No item specified";
                return this.RedirectToGrocery();
            }
            //TempData["ErrorMessage"] = null; //$"Unable to edit item ({itemId})";
            //return this.RedirectToGrocery();
            var model = await _repo.GetItemAsync(homeId, itemId, cancel);
            return View(model?.ToFormModel());
        }
        [HttpPost("edit/{itemId}")]
        public async Task<IActionResult> Edit([FromRoute] string itemId, [FromForm] GroceryItemForm formModel, CancellationToken cancel)
        {
            //this.SetHomeId(homeId);
            if (string.IsNullOrWhiteSpace(itemId) || formModel == null || string.IsNullOrWhiteSpace(formModel.Id))
            {
                TempData["ErrorMessage"] = $"No item specified";
                return this.RedirectToGrocery();
            }
            if (!itemId.Equals(formModel.Id, StringComparison.Ordinal) || !homeId.Equals(formModel.HomeId, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = $"Invalid item specified";
                return this.RedirectToGrocery();
            }
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = $"Unable to edit item ({itemId}): check details below";
                return View(formModel);
            }
            try
            {
                var model = formModel.ToModel(homeId);
                if (formModel.AddToCart)
                {
                    AddToCart(model);
                }
                await _repo.AddAsync(model, cancel);
                TempData["InfoMessage"] = $"{formModel.Name} edited";
                TempData["ErrorMessage"] = null;
                return this.RedirectToGrocery();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Edit Error ({0}): {0}", homeId, formModel);
                ViewData["ErrorMessage"] = "Unable to edit the item";
            }
            return View(formModel);
        } // END Edit

        [HttpGet("delete/{itemId}")]
        public async Task<IActionResult> Delete([FromRoute] string itemId, CancellationToken cancel)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                TempData["ErrorMessage"] = "No item specified";
                return this.RedirectToGrocery();
            }

            try
            {
                var model = await _repo.GetItemAsync(homeId, itemId, cancel);
                return View(model);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Delete.Get Error: {0}|{1}", homeId, itemId);
                TempData["ErrorMessage"] = $"Unable to remove the item: {itemId}";
            }
            return this.RedirectToGrocery();
        } // END Delete
        // Post is confirmation
        [HttpPost("delete/{itemId}")]
        public async Task<IActionResult> Delete([FromRoute] string itemId, [FromForm] Lib.Models.GroceryItem model, CancellationToken cancel)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                TempData["ErrorMessage"] = "No item specified";
                return this.RedirectToGrocery();
            }
            if (!itemId.Equals(model.Id, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "Mismatch with item specified";
                return this.RedirectToGrocery();
            }

            try
            {
                //TempData["ErrorMessage"] = $"Unable to delete item ({itemId}) from list";
                var result = await _repo.DeleteAsync(model, cancel);
                TempData["InfoMessage"] = $"Item ({result?.Name}) removed from list";
                TempData["ErrorMessage"] = null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Delete Error: {0}|{1}", homeId, itemId);
                ViewData["ErrorMessage"] = "Unable to remove the item";
            }

            return this.RedirectToGrocery();
        } // END Delete

        // THIS NEEDS TO BE POST ONLY
        [Route("tocart/{itemId}")]
        public async Task<IActionResult> ToCart(string itemId, CancellationToken cancel)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                TempData["ErrorMessage"] = $"No item specified";
                return this.RedirectToGrocery();
            }
            //TempData["ErrorMessage"] = $"Unable to move item ({itemId}) to cart";

            try
            {
                var model = await _repo.GetItemAsync(homeId, itemId, cancel);
                if (model == null)
                {
                    TempData["ErrorMessage"] = $"Missing grocery item ({itemId})";
                    return this.RedirectToGrocery();
                }
                AddToCart(model);
                model = await _repo.AddAsync(model, cancel);
                TempData["InfoMessage"] = $"{model?.Name} added to cart";
                TempData["ErrorMessage"] = null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Delete Error: {0}|{1}", homeId, itemId);
                ViewData["ErrorMessage"] = "Unable to remove the item";
            }

            return this.RedirectToGrocery();
        }

        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout(CancellationToken cancel)
        {
            //TempData["ErrorMessage"] = $"Unable to checkout cart";

            try
            {
                // checkout all items currently in cart
                var list = await _repo.GetCheckoutAsync(homeId, cancel);

                if (list == null || !list.Any())
                {
                    ViewData["ErrorMessage"] = "There are no items in your cart";
                    return this.RedirectToGrocery();
                }

                return View(new CheckoutForm
                {
                    HomeId = homeId,
                    Items = list.AsList(),
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Checkout.Get Error: {0}", homeId);
                ViewData["ErrorMessage"] = "Checkout failed";
            }
            return this.RedirectToGrocery();
        } // END Checkout

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutForm model, CancellationToken cancel)
        {
            //if (model == null) { }
            if (homeId != model.HomeId)
            {
                _log.LogError("Home `{0}` does NOT match model home ID `{1}` :: CHECKOUT: {2}", homeId,
                    model.HomeId, System.Text.Json.JsonSerializer.Serialize(model));
                TempData["ErrorMessage"] = "Checkout NOT Valid";
                return this.RedirectToGrocery();
            }

            IEnumerable<Lib.Models.GroceryItem>? list = null;

            try
            {
                // checkout all items currently in cart
                list = await _repo.CheckoutAsync(homeId, model.ItemIds, model.StoreName, cancel);
                TempData["InfoMessage"] = $"{list.Count()} items marked as purchased";
                TempData["ErrorMessage"] = null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Checkout Error: {0} (InCart: {1})", homeId, list);
                TempData["ErrorMessage"] = "Checkout failed";
            }
            return this.RedirectToGrocery();
        } // END Checkout

        [HttpGet("previous-trips")]
        [HttpGet("previoustrips")]
        public async Task<IActionResult> PreviousTrips(CancellationToken cancel)
        {
            var result = new Models.Forms.GroceryTripForm();
            try
            {
                var list = await _repo.GetTripsAsync(homeId, cancel);
                if (list.Any()) result.Items = list;
                TempData["InfoMessage"] = $"{list.Count()} trips found";
                TempData["ErrorMessage"] = null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "PreviousTrips Error: {0}", homeId);
                ViewData["ErrorMessage"] = "Unable to retrieve previous trips";
            }
            return View(result);
        }
    }
}
