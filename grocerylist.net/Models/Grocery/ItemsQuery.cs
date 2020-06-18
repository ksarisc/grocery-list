using System;

namespace grocerylist.net.Models.Grocery
{
    public class ItemsQuery
    {
        public string Name { get; set; }
        public DateTime? CreatedDate1 { get; set; }
        public DateTime? CreatedDate2 { get; set; }
        public DateTime? PurchasedDate1 { get; set; }
        public DateTime? PurchasedDate2 { get; set; }
    }
}
