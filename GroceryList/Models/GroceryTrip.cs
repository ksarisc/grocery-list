using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models
{
    public class GroceryTrip
    {
        [Required]
        [StringLength(100, MinimumLength = 10)]
        public string Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 20)]
        public string HomeId { get; set; }
        [Required]
        public DateTimeOffset CheckoutTime { get; set; }
    }
}
