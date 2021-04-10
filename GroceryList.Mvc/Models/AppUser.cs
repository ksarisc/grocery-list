using System;

namespace GroceryList.Mvc.Models
{
    public class AppUser
    {
        public Guid HomeId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
