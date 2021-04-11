using GroceryList.Mvc.Models;
using System;

namespace GroceryList.Mvc
{
    internal static class Utilities
    {
        private const string guidFormat = "D";

        public static string GetHome(this AppUser self)
        {
            // to string uses `D` format
            return self.HomeId.ToString(guidFormat);
        }

        public static string GetHome(this TripList self)
        {
            return self.HomeId.ToString(guidFormat);
        }
    }
}
