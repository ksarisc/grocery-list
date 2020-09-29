using System;

namespace grocerylist.net.Models.Grocery
{
    public class Item
    {
        public uint Id { get; set; } = 0;
        public uint HomeId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? RequestedTime { get; set; }

        public string Notes { get; set; }
    }
}
