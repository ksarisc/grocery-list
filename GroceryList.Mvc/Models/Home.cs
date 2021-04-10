using System;

namespace GroceryList.Mvc.Models
{
    public class Home
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
