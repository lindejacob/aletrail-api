using System.ComponentModel.DataAnnotations;

namespace aletrail_api.Models;

public class Challenge
{
    [Key]
    public int Id { get; set; }
    
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public ChallengeType Type { get; set; }
    
    public int? Points { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
}

public enum ChallengeType
{
    Drink,
    Game,
    Photo,
    Trivia,
    Social,
    Custom
}
