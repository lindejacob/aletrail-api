using aletrail_api.Models;

namespace aletrail_api.Dtos.PubCrawl;

public class ChallengeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ChallengeType Type { get; set; }
    public int? Points { get; set; }
}

public class CreateChallengeDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ChallengeType Type { get; set; }
    public int? Points { get; set; }
}

public class UpdateChallengeDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public ChallengeType? Type { get; set; }
    public int? Points { get; set; }
}
