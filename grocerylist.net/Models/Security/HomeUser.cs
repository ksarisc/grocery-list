using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace grocerylist.net.Models.Security
{
    public class HomeUser : IdentityUser
    {
        //public int Id { get; set; }
        public int HomeId { get; set; }
        public int? AdminHomeId { get; set; }
        [Required]
        [StringLength(75, MinimumLength = 3)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(75, MinimumLength = 3)]
        public string LastName { get; set; }
    }
}
