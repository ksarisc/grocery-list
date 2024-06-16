using System;
using System.Collections.Generic;

namespace GroceryList.Models.Forms
{
    public class GroceryTripForm
    {
        public IEnumerable<Lib.Models.GroceryTrip> Items { get; set; } = [];
    }
}
