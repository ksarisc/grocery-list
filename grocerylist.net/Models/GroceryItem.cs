using System;

namespace grocerylist.net.Models
{
    public class GroceryItem
    {
        public int Id { get; set; } = 0;
        public int HomeId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RequestedAt { get; set; }
        public string RequestedBy { get; set; }
    }
}
