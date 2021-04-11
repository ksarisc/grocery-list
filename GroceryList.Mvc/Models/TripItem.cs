using System;

namespace GroceryList.Mvc.Models
{
    public class TripItem
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? RequestedTime { get; set; }
    }
}
