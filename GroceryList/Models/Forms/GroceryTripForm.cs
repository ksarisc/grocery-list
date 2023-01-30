using System;
using System.Collections.Generic;

namespace GroceryList.Models.Forms
{
    public class GroceryTripForm
    {
        public IEnumerable<GroceryTrip> Items { get; set; } = Array.Empty<GroceryTrip>();
    }
}
