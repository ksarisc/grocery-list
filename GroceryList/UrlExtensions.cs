using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace GroceryList;

public static class UrlExtensions
{
    private const string index = "Index";

    public static string? GetHomeId(this HttpContext self)
    {
        var homeId = self.Request.Cookies[cookieId];
        // when just set, will be in items
        if (string.IsNullOrEmpty(homeId)) homeId = self.Items[cookieId] as string;

        return !string.IsNullOrWhiteSpace(homeId) ? homeId : null;
    }
    public static string? GetHomeId(this Controller self)
    {
        return self.HttpContext.GetHomeId();
    }
    public static string? GetHomeId(this ViewContext self)
    {
        return self.HttpContext.GetHomeId();
    }

    public static string? GetHomeUrl(this IUrlHelper self, ViewContext context)
    {
        var homeId = context.GetHomeId();
        if (String.IsNullOrEmpty(homeId))
        {
            //asp-area=""
            return self.Action(index, "Home");
        }
        return self.Action(index, "Grocery", new { homeId = homeId, });
    } // END GetHomeUrl

    public static string? GetGroceryUrl(this IUrlHelper self, ViewContext context,
        string? action = null, string? itemId = null)
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
        var homeId = context.GetHomeId();
        if (homeId != null) values.Add("homeId", homeId);
        if (!string.IsNullOrWhiteSpace(itemId))
        {
            values.Add("itemId", itemId);
        }
        return self.Action(action, "Grocery", values);
    } // END GetGroceryUrl
    public static string? GetGroceryUrl(this Controller self, string? action = null)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            action = index;
        }
        return self.Url.Action(action, "Grocery", new { homeId = self.GetHomeId(), });
    }

    public static IActionResult RedirectToGrocery(this Controller self) //, string homeId = null)
    {
        // ?? If home ID is NULL, send to normal index ??

        // if (!string.IsNullOrWhiteSpace(homeId)){
        //     self.SetHomeId(homeId);
        // }else{
        //     homeId = self.GetHomeId();
        // }
        //  return self.RedirectToAction(index, "Grocery", new { homeId = homeId, });
        return self.RedirectToAction(index, "Grocery", new { homeId = self.GetHomeId(), });
    }
    public static IActionResult RedirectToGrocery(this Controller self, string homeId, string homeTitle)
    {
        self.HttpContext.SetHome(homeId, homeTitle);
        return self.RedirectToAction(index, "Grocery", new { homeId = self.GetHomeId(), });
    }

    private const string cookieId = "HomeId";
    private const string cookieTitle = "HomeTitle";
    public static string GetHomeTitle(this HttpContext self)
    {
        var homeTitle = self.Request.Cookies[cookieTitle];
        if (!string.IsNullOrEmpty(homeTitle)) return homeTitle;

        return "Grocery List"; //Groceries";
    }
    public static HttpContext SetHome(this HttpContext self, string homeId, string homeTitle)
    {
        if (!string.IsNullOrWhiteSpace(homeId))
        {
            // if (homeId.Equals(self.Request.Cookies[cookieId], StringComparison.Ordinal) ||
            //     homeTitle.Equals(self.Request.Cookies[cookieTitle], StringComparison.Ordinal)){
            //     return self;
            // }
            self.Items[cookieId] = homeId;
            self.Items[cookieTitle] = homeTitle;

            var expires = new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7), };
            self.Response.Cookies.Append(cookieId, homeId, expires);
            self.Response.Cookies.Append(cookieTitle, homeTitle, expires);
        }
        else
        {
            self.Response.Cookies.Delete(cookieId);
            self.Response.Cookies.Delete(cookieTitle);
        }
        return self;
    }
}
