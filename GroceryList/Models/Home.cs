using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models
{
    public class Home
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public DateTimeOffset CreatedTime { get; set; }
        [Required]
        public string CreatedByMeta { get; set; }
    }
}
