using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Lib.Models
{
    public class GroceryTrip
    {
        [Required]
        [StringLength(100, MinimumLength = 10)]
        public string Id { get; set; } = string.Empty;
        [Required]
        [StringLength(50, MinimumLength = 20)]
        public string HomeId { get; set; } = string.Empty;
        [Required]
        public DateTimeOffset CheckoutTime { get; set; }

        public string? StoreName { get; set; }
    }
}
