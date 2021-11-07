using GroceryList.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GroceryList.Data
{
    public class GroceryContext : IdentityDbContext
    {
        public DbSet<GroceryItem> Current { get; set; }
        public DbSet<PurchasedItem> Purchased { get; set; }

        public GroceryContext(DbContextOptions<GroceryContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GroceryItem>().HasIndex(g => new { g.HomeId, g.Name }).IsUnique();
            builder.Entity<PurchasedItem>().HasIndex(g => new { g.HomeId, g.Name, g.PurchasedOn }).IsUnique();

            base.OnModelCreating(builder);
        }
    }
}
