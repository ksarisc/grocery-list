using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models
{
    public class Home
    {
        [Key]
        [StringLength(50, MinimumLength = 10)]
        public string Id { get; set; }

        public DateTimeOffset CreatedTime { get; set; }
        [Required]
        [StringLength(256, MinimumLength = 10)]
        public string CreatedUser { get; set; }
    }
}
