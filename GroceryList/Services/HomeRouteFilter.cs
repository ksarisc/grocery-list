using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryList.Services
{
    internal class HomeRouteFilter : IAsyncActionFilter
    {
        public const string Label = "HomeId";
        public const string Parameter = "homeSlug";
        public const string Route = "~/{homeId}/[controller]"; // should this start with ~

        public const string HasHome = "HasHome";

        public HomeRouteFilter() { }

        private string? GetFromModelState(ActionExecutingContext context)
        {
            if (context.ModelState.TryGetValue(Parameter, out var value) &&
                value.RawValue is string homeSlug &&
                !string.IsNullOrWhiteSpace(homeSlug))
            {
                return homeSlug;
            }
            return null;
        } // END GetFromModelState

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var home = GetFromModelState(context);
            if (home == null)
            {
                home = context.RouteData.Values
                    .FirstOrDefault(r => r.Key.Equals(Parameter, StringComparison.Ordinal))
                    .Value as string;
            }

            // execute MVC filter pipeline
            var resultContext = await next();

            if (resultContext.Result is ViewResult view)
            {
                // ?? TempData ??
                view.ViewData[Label] = home;
            }
        } // END OnActionExecutionAsync

        public static string? GetSlug(HttpContext? context = null)
        {
            if (context?.Request?.RouteValues?.ContainsKey(Parameter) == true)
            {
                return context.Request.RouteValues[Parameter] as string;
            }
            return null;
        }
    }
}
