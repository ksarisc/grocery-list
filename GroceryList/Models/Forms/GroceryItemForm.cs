using System;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Models.Forms
{
    public class GroceryItemForm
    {
        [StringLength(50, MinimumLength = 20)]
        public string? Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 20)]
        public string? HomeId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string? Name { get; set; }
        [StringLength(50, MinimumLength = 2)]
        public string? Brand { get; set; }
        [StringLength(1000, MinimumLength = 4)]
        public string? Notes { get; set; }

        [Range(.001, 1000, ErrorMessage = "Price MUST be between .001 & 1000.00")]
        public double? Price { get; set; }
        [Range(1, 10000, ErrorMessage = "Qty MUST be between 1 & 10000")]
        public int? Qty { get; set; }
        public bool AddToCart { get; set; } = false;

        // IGNORE THE REST OF THE VALUES, THEY ARE SET BY THE SERVER
        public DateTimeOffset? CreatedTime { get; set; }
        public string? CreatedUser { get; set; }
        public DateTimeOffset? InCartTime { get; set; }
        public string? InCartUser { get; set; }
        public DateTimeOffset? PurchasedTime { get; set; }
        public string? PurchasedUser { get; set; }
    }

    public static class GroceryItemFormExtensions
    {
        public static GroceryItemForm ToFormModel(this GroceryItem self)
        {
            return new GroceryItemForm
            {
                Id = self.Id,
                HomeId = self.HomeId,
                Name = self.Name,
                Brand = self.Brand,
                Notes = self.Notes,
                Price = self.Price,
                Qty = self.Qty,
                //AddToCart=self.AddToCart,

                CreatedTime = self.CreatedTime,
                CreatedUser = self.CreatedUser,
                InCartTime = self.InCartTime,
                InCartUser = self.InCartUser,
                PurchasedTime = self.PurchasedTime,
                PurchasedUser = self.PurchasedUser,
            };
        }
        public static GroceryItem ToModel(this GroceryItemForm self, string homeId)
        {
            if (string.IsNullOrWhiteSpace(self.HomeId))
            {
                self.HomeId = homeId;
            }
            else if (!string.Equals(self.HomeId, homeId, StringComparison.Ordinal))
            {
                throw new ArgumentOutOfRangeException($"Home ID ({self.HomeId}) NOT Valid!");
            }

            if (string.IsNullOrWhiteSpace(self.Name))
            {
                throw new ArgumentOutOfRangeException(nameof(GroceryItemForm.Name));
            }

            return new GroceryItem
            {
                Id = self.Id, //?? string.Empty,
                HomeId = self.HomeId,
                Name = self.Name.Trim(),
                Brand = self.Brand?.Trim(),
                Notes = self.Notes?.Trim(),
                Price = self.Price,
                Qty = self.Qty,
                //AddToCart=self.AddToCart,

                // // IGNORE THE REST OF THE VALUES, THEY ARE SET BY THE SERVER
                // CreatedTime
                // CreatedUser
                // InCartTime
                // InCartUser
                // PurchasedTime
                // PurchasedUser
            };
        }
    }
}
