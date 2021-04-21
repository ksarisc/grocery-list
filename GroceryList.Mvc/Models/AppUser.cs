using System;

namespace GroceryList.Mvc.Models
{
    public class AppUser
    {
        public Guid? Id { get; set; }
        public Guid? HomeId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string IpAddress { get; set; }

        public DateTime? CreatedTime { get; set; }

        public bool Confirmed { get; set; }

        public static readonly AppUser Empty = new AppUser
        {
        };
    }
}
