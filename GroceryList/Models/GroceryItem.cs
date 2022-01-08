using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models
{
    public class GroceryItem
    {
        [Required]
        [StringLength(50, MinimumLength = 20)]
        public string HomeId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Name { get; set; }
        [StringLength(50, MinimumLength = 2)]
        public string Brand { get; set; }
        // ?? Metadata ??
        [StringLength(1000, MinimumLength = 4)]
        public string Notes { get; set; }

        public double? Price { get; set; }

        [Required]
        public DateTimeOffset CreatedTime { get; set; }
        [Required]
        public string CreatedUser { get; set; }
        public DateTimeOffset? InCartTime { get; set; }
        public string InCartUser { get; set; }
        public DateTimeOffset? PurchasedTime { get; set; }
        public string PurchasedUser { get; set; }
    }
}
