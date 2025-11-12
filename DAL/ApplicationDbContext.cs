using aletrail_api.Models;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.DAL;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
}

