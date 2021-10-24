using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace grocerylist.net.Models
{
    public class Home
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid PublicId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Name { get; set; }
        [Required]
        public int CreatedBy { get; set; }
        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        public virtual ICollection<Security.HomeUser> Users { get; set; }
        public virtual ICollection<Security.HomeUser> Admins { get; set; }
    }
}
