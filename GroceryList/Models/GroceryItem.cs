using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models
{
    public class BaseItem
    {
        [Required]
        public int HomeId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Name { get; set; }
        [StringLength(100, MinimumLength = 4)]
        public string Brand { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string CreatedBy { get; set; }
        [Required]
        public DateTimeOffset CreatedOn { get; set; }
    }
    public class GroceryItem : BaseItem
    {
        public int Id { get; set; }
    }
}
