using System;

namespace GroceryList.Mvc.Models.Forms
{
    public class TripItemRequest
    {
        public string ItemName { get; set; }
        public string ItemBrand { get; set; }
        public string RequestedTime { get; set; }
        public string Save { get; set; }
    }
}
