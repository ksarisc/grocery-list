using GroceryList.Lib.Models;
using System;

namespace GroceryList.Models
{
    public class GroceryTripData
    {
        public string? StoreName { get; set; }
        public GroceryItem[] Items { get; set; } = [];
    }
}
