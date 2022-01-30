using System;
using System.Text.Json.Serialization;

namespace GroceryList.Models
{
    public interface IAuthModel
    {
        public string Id { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? EditedTime { get; set; }
    }
}
