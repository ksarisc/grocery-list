using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace grocerylist.net.Models.Security
{
    public class HomeUser : IdentityUser
    {
        public uint HomeId { get; protected set; }
        public string HomeIdHash { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        // public static HomeUser Get(ClaimsPrincipal user) //IdentityUser user)
        // {
        //     // should do more here to setup correctly, but for now
        //     return user.Identity as HomeUser;
        // }
    }
}
