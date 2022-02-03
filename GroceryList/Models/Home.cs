using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models
{
    public class Home
    {
        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Id { get; set; }
        [StringLength(100, MinimumLength = 4)]
        public string Title { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 4)]
        public string CreatedBy { get; set; }
        [Required]
        public DateTimeOffset CreatedTime { get; set; }
        [Required]
        public string CreatedByMeta { get; set; }
    }
}
