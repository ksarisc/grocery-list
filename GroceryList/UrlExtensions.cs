using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace GroceryList
{
    public static class UrlExtensions
    {
        private const string index = "Index";

        public static void SetHomeId(this Controller self, string homeId)
        {
            self.TempData["HomeId"] = homeId;
            self.ViewData["HomeId"] = homeId;
        }

        public static string GetHomeId(this Controller self)
        {
            if (self.TempData["HomeId"] != null)
            {
                return self.TempData["HomeId"] as string;
            }
            return self.ViewData["HomeId"] as string;
        }
        public static string GetHomeId(this ViewContext self)
        {
            if (self.TempData["HomeId"] != null)
            {
                return self.TempData["HomeId"] as string;
            }
            return self.ViewData["HomeId"] as string;
        }

        public static string GetHomeUrl(this IUrlHelper self, ViewContext context)
        {
            var homeId = context.GetHomeId();
            if (String.IsNullOrEmpty(homeId))
            {
                //asp-area=""
                return self.Action(index, "Home");
            }
            return self.Action(index, "Grocery", new { homeId = homeId, });
        } // END GetHomeUrl

        public static string GetGroceryUrl(this IUrlHelper self, ViewContext context,
            string action = null, string itemId = null)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                action = index;
            }
            //object values;
            //if (string.IsNullOrWhiteSpace(itemId)){
            //    values = new { homeId = context.GetHomeId(), };
            //}else{
            //    values = new { homeId = context.GetHomeId(), itemId = itemId, };
            //}
            var values = new System.Collections.Generic.Dictionary<string, string>();
            values.Add("homeId", context.GetHomeId());
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                values.Add("itemId", itemId);
            }
            return self.Action(action, "Grocery", values);
        } // END GetGroceryUrl
        public static string GetGroceryUrl(this Controller self, string action = null)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                action = index;
            }
            return self.Url.Action(action, "Grocery", new { homeId = self.GetHomeId(), });
        }

        public static IActionResult RedirectToGrocery(this Controller self, string homeId = null)
        {
            if (!string.IsNullOrWhiteSpace(homeId))
            {
                self.SetHomeId(homeId);
            }
            else
            {
                homeId = self.GetHomeId();
            }
            return self.RedirectToAction(index, "Grocery", new { homeId = homeId, });
        }
    }
}
