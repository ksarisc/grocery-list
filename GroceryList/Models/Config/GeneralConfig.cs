using System;

namespace GroceryList.Models.Config
{
    public class GeneralConfig
    {
        public bool AllowHomeCreation { get; set; }
        public string[]? AllowedIpAddresses { get; set; }
    }
}
