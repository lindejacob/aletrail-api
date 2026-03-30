using System.ComponentModel.DataAnnotations;

namespace aletrail_api.Models;

public class PubCrawlRoute
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public string InviteCode { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<RouteStop> Stops { get; set; } = new List<RouteStop>();
    public ICollection<PubCrawlParticipant> Participants { get; set; } = new List<PubCrawlParticipant>();
}
