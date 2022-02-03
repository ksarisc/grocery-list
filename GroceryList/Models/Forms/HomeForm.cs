using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models.Forms
{
    public class HomeForm
    {
        [StringLength(100, MinimumLength = 4)]
        public string Title { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 4)]
        public string CreatedBy { get; set; }
    }
}
