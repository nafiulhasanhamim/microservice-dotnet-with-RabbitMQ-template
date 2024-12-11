using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
namespace ProductAPI;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // builder.Entity<Order>()
        // .HasOne(o => o.User)
        // .WithMany(u => u.Orders)
        // .HasForeignKey(o => o.UserId);

    }
    public DbSet<Product> Products { get; set; }

}
