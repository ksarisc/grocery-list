using System;
using System.Security.Claims;

namespace grocerylist.net.Models
{
    public class HomeUser : ClaimsPrincipal
    {
        public int Id { get; protected set; }
        public int HomeId { get; protected set; }
    }
}
