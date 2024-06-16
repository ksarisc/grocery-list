using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GroceryList.Models.Forms
{
    public class CheckoutForm
    {
        [Required]
        [StringLength(50, MinimumLength = 20)]
        public string? HomeId { get; set; }
        [StringLength(100, MinimumLength = 2)]
        public string? StoreName { get; set; }

        [Required]
        public string JoinedIds
        {
            get { return string.Join('|', ItemIds); }
            set
            {
                ItemIds.Clear();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ItemIds.AddRange(value.Split('|', StringSplitOptions.RemoveEmptyEntries));
                }
            }
        }
        public List<string> ItemIds { get; } = new List<string>();
        public List<Lib.Models.GroceryItem>? Items { get; set; }
    }
}
