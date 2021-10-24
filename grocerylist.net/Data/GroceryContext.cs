using grocerylist.net.Models;
using grocerylist.net.Models.Grocery;
using grocerylist.net.Models.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace grocerylist.net.Data
{
    public class GroceryContext : DbContext
    {
        public DbSet<Home> Homes { get; set; }
        public DbSet<HomeUser> HomeUsers { get; set; }

        public DbSet<CurrentItem> Current { get; set; }
        public DbSet<PurchasedItem> Purchases { get; set; }
        public DbSet<SavedItem> SaveForLater { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CurrentItem>().HasIndex(i => new { i.HomeId, i.Name }).IsUnique();

            base.OnModelCreating(builder);
        }
    }
}
