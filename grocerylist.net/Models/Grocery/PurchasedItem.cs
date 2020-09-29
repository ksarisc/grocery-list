using System;

namespace grocerylist.net.Models.Grocery
{
    public class PurchasedItem : Item
    {
        public DateTime PurchasedTime { get; set; }
        public string PurchasedBy { get; set; }
    }
}
