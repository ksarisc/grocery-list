using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GroceryList.Models
{
    //public class GroceryItem
    //{
    //    [Required]
    //    [StringLength(50, MinimumLength = 20)]
    //    public string? Id { get; set; }
    //    [Required]
    //    [StringLength(50, MinimumLength = 20)]
    //    public string HomeId { get; set; }
    //    [Required]
    //    [StringLength(50, MinimumLength = 4)]
    //    public string Name { get; set; }
    //    [StringLength(50, MinimumLength = 2)]
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public string? Brand { get; set; }
    //    // ?? Metadata ??
    //    [StringLength(1000, MinimumLength = 4)]
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public string? Notes { get; set; }

    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public double? Price { get; set; }
    //    // ?? assume NULL is 1 ??
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public int? Qty { get; set; }

    //    [Required]
    //    public DateTimeOffset CreatedTime { get; set; }
    //    [Required]
    //    public string CreatedUser { get; set; }
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public DateTimeOffset? InCartTime { get; set; }
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public string? InCartUser { get; set; }
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public DateTimeOffset? PurchasedTime { get; set; }
    //    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    //    public string? PurchasedUser { get; set; }
    //}
}
