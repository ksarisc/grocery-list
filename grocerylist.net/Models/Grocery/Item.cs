using System;

namespace grocerylist.net.Models.Grocery
{
    public class Item
    {
        public int Id { get; set; } = 0;
        public int HomeId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? RequestedTime { get; set; }

        public string Notes { get; set; }
    }
}
