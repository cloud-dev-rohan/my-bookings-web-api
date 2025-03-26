using Microsoft.EntityFrameworkCore;
using MyBookingsWebApi.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MyBookingsWebApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        { }

        public DbSet<Member> Members { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure primary keys, relationships etc. if needed.
        }
    }
}
