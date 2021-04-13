using System;
using System.Collections.Generic;

namespace GroceryList.Mvc.Models
{
    public class TripList
    {
        public Guid HomeId { get; }

        public List<TripItem> Items { get; } = new List<TripItem>();

        public TripList(Guid homeId)
        {
            HomeId = homeId;
        }
    }
}
