using System.ComponentModel.DataAnnotations;

namespace grocerylist.net.ModelViews.Home
{
    public class CreateData
    {
        [Required]
        [StringLength(50, MinimumLength = 5)]
        public string Name { get; set; }
    }
}
