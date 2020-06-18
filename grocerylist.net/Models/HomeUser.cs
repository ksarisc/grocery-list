using System;
using Microsoft.AspNetCore.Identity;

namespace grocerylist.net.Models
{
    public class HomeUser : IdentityUser
    {
        public int HomeId { get; protected set; }
        public string HomeIdHash { get; set; }
    }
}
