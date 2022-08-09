using System;
using System.Text.Json.Serialization;

namespace GroceryList.Models
{
    #nullable disable
    public class AppRole
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? EditedTime { get; set; }

        public static AppRole Empty { get; } = new AppRole
        {
            Id = string.Empty,
            Name = string.Empty,
            NormalizedName = string.Empty,
            CreatedTime = DateTimeOffset.MinValue,
        };
    }
    #nullable enable
}
