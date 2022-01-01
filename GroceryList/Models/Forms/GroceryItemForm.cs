using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models.Forms
{
    public class GroceryItemForm
    {
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Name { get; set; }
        [StringLength(50, MinimumLength = 2)]
        public string Brand { get; set; }
        [StringLength(1000, MinimumLength = 4)]
        public string Notes { get; set; }
    }
}
