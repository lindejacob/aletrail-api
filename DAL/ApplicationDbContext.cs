using aletrail_api.Models;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.DAL;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Bar> Bars { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Bar>().HasKey(b => b.OsmId);
    }
}
