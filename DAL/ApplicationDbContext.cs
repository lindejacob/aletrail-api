using aletrail_api.Models;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.DAL;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Bar> Bars { get; set; } = null!;
    public DbSet<PubCrawlRoute> PubCrawlRoutes { get; set; } = null!;
    public DbSet<RouteStop> RouteStops { get; set; } = null!;
    public DbSet<PubCrawlParticipant> PubCrawlParticipants { get; set; } = null!;
    public DbSet<Challenge> Challenges { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Bar>().HasKey(b => b.OsmId);
        
        modelBuilder.Entity<PubCrawlRoute>()
            .HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<RouteStop>()
            .HasOne(rs => rs.PubCrawlRoute)
            .WithMany(r => r.Stops)
            .HasForeignKey(rs => rs.PubCrawlRouteId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RouteStop>()
            .HasOne(rs => rs.Bar)
            .WithMany()
            .HasForeignKey(rs => rs.BarOsmId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<RouteStop>()
            .HasOne(rs => rs.Challenge)
            .WithMany(c => c.RouteStops)
            .HasForeignKey(rs => rs.ChallengeId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<RouteStop>()
            .HasIndex(rs => new { rs.PubCrawlRouteId, rs.OrderIndex })
            .IsUnique();
        
        modelBuilder.Entity<PubCrawlParticipant>()
            .HasOne(p => p.PubCrawlRoute)
            .WithMany(r => r.Participants)
            .HasForeignKey(p => p.PubCrawlRouteId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PubCrawlParticipant>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PubCrawlParticipant>()
            .HasIndex(p => new { p.PubCrawlRouteId, p.UserId })
            .IsUnique();
        
        modelBuilder.Entity<PubCrawlRoute>()
            .HasIndex(r => r.InviteCode)
            .IsUnique();
    }
}
