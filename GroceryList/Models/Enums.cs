using System;

namespace GroceryList.Models
{
    //public class Enums { }

    public enum Roles
    {
        HasHome,
    }

    public static class Enums
    {
        public static string AsString(this Roles self)
        {
            return self.ToString();
        }
    }
}
