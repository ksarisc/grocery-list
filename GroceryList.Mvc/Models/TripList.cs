using System;
using System.Collections.Generic;

namespace GroceryList.Mvc.Models
{
    public class TripList
    {
        public Guid HomeId { get; set; }

        public IEnumerable<TripItem> Items { get; set; }
    }
}
