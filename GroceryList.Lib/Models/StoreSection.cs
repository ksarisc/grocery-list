using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GroceryList.Lib.Models
{
    public class StoreSection
    {
        public int Order { get; set; } = 0;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Label { get; set; }

        [StringLength(1000, MinimumLength = 2)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
    }
}
