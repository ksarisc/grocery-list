using System;
using System.ComponentModel.DataAnnotations;

namespace grocerylist.net.Models.Grocery
{
    public class Item
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int HomeId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Name { get; set; }
        [StringLength(150)]
        public string Brand { get; set; }
        [Required]
        //[Default]
        public DateTimeOffset CreatedTime { get; set; }
        [Required]
        public int CreatedBy { get; set; }
        public DateTimeOffset? RequestedTime { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }

        public virtual Home Home { get; set; }
        public virtual Security.HomeUser CreatedByUser { get; set; }
    }
}
