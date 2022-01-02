using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models
{
    public class HomeUser
    {
        [Key]
        [StringLength(50, MinimumLength = 10)]
        public string HomeId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [StringLength(256, MinimumLength = 2)]
        public string UserName { get; set; }

        public DateTimeOffset? ApprovedTime { get; set; }
        public string ApprovedUser { get; set; }
    }
}
