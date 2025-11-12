using Microsoft.EntityFrameworkCore;

namespace aletrail_api.DAL;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserModel> Users { get; set; } = null!;
}

