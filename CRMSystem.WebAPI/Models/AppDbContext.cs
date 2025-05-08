using Microsoft.EntityFrameworkCore;

namespace CRMSystem.WebAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Florist> Florists { get; set; }
        public DbSet<Flower> Flowers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Order> Orders { get; set; }

    }
}
