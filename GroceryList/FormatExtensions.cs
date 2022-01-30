using System;

namespace GroceryList
{
    public static class FormatExtensions
    {
        public static string Format(this DateTimeOffset self, bool includeDate = true)
        {
            // assign user's timezone?
            if (includeDate)
            {
                return self.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return self.ToString("HH:mm:ss");
        }
        public static string Format(this DateTimeOffset? self, bool includeDate = true)
        {
            if (self == null) return "";
            return Format(self.Value, includeDate);
        }
    }
}
