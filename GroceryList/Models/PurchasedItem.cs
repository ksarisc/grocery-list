using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models
{
    public class PurchasedItem : BaseItem
    {
        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string PurchasedBy { get; set; }
        [Required]
        public DateTimeOffset PurchasedOn { get; set; }
    }
}
