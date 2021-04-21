using GroceryList.Mvc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Text.Json;

namespace GroceryList.Mvc
{
    internal static class Utilities
    {
        private const string guidFormat = "D";
        public static string ToId(this Guid self)
        {
            return self.ToString(guidFormat);
        }

        public static string GetId(this AppUser self)
        {
            // to string uses `D` format
            return self.Id.Value.ToString(guidFormat);
        }

        public static string GetHome(this AppUser self)
        {
            return self.HomeId.Value.ToString(guidFormat);
        }

        public static string GetHome(this TripList self)
        {
            return self.HomeId.ToString(guidFormat);
        }

        private static AppUser me;
        internal static void SetUser(IConfiguration conf)
        {
            var debugUser = conf.GetSection("DebugUser");
            if (!debugUser.Exists()) return;
            me = new AppUser();
            debugUser.Bind(me);
        }

        public static AppUser GetUser(this HttpRequest self)
        {
            var context = self.HttpContext;
            var remote = context.Connection.RemoteIpAddress;
            var local = context.Connection.LocalIpAddress;
            //if (!address.Equals("::1") && !address.Equals("127.0.0.1"))
            if (!remote.Equals(local))
            {
                Console.Out.WriteLine($"EMPTY USER FOUND: {remote}|{local}");
                return AppUser.Empty;
            }
            Console.Out.WriteLine($"I WAS FOUND: {remote}|{local}");
            return context.User.ToAppUser();
        }

        private static AppUser ToAppUser(this ClaimsPrincipal self)
        {
            // var user = new AppUser();
            // // foreach (var claim in self.Claims)
            // // {
            // //     claim.Type
            // // }
            // return null;
            return me;
        }

        public static bool IsEmpty(this AppUser self)
        {
            return self == null || self == AppUser.Empty;
        }

        public static string GetContext(this AppUser self)
        {
            if (self.IsEmpty())
            {
                return "(USER NOT FOUND)";
            }
            return $"(Id:{self.GetId()})(Home:{self.GetHome()})(Email:{self.Email})";
        }
    }
}
