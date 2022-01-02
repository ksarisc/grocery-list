using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models.Identity
{
    public class GroceryUser : IdentityUser
    {
        // NOT REQUIRED because user will identify with home once logged in?
        // OR
        // IS REQUIRED because user MUST identify before logging in?
        [StringLength(50, MinimumLength = 10)]
        public string HomeId { get; set; }
    }
}
