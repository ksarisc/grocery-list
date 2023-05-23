using System;
using System.Text.Json.Serialization;

namespace GroceryList.Models
{
    #nullable disable
    // I'm planning to use Google, Facebook, Microsoft, or some other
    // auth service, so don't really want to store much locally
    public class AppUser
    {
        public string Id { get; set; }
        public string HomeId { get; set; }

        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }

        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTimeOffset CreatedTime { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? EditedTime { get; set; }

        public static AppUser Empty { get; } = new AppUser
        {
            Id = string.Empty,
            UserName = string.Empty,
            NormalizedUserName = string.Empty,
            Email = string.Empty,
            NormalizedEmail = string.Empty,
            EmailConfirmed = false,
            PasswordHash = string.Empty,
            PhoneNumber = string.Empty,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            CreatedTime = DateTimeOffset.MinValue,
        };
    }
    #nullable enable
}
