using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Admin.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Flower> Flowers { get; set; }
        public DbSet<Florist> Florists { get; set; }
        public DbSet<Company> Companies { get; set; }
    }
}
