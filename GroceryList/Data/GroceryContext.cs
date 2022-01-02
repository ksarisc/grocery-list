using GroceryList.Models;
using GroceryList.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GroceryList.Data
{
    public class GroceryContext : IdentityDbContext<GroceryUser>
    {
        public DbSet<Home> Homes { get; set; }
        public DbSet<HomeUser> HomeUsers { get; set; }
        public DbSet<GroceryItem> Current { get; set; }
        public DbSet<PurchasedItem> Purchased { get; set; }

        public GroceryContext(DbContextOptions<GroceryContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GroceryItem>().HasIndex(g => new { g.HomeId, g.Name }).IsUnique();
            builder.Entity<PurchasedItem>().HasNoKey().HasIndex(g => new { g.HomeId, g.Name, g.PurchasedOn }).IsUnique();

            base.OnModelCreating(builder);
        }
    }
}
