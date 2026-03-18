using aletrail_api.Dtos.PubCrawl;
using aletrail_api.Models;

namespace aletrail_api.Mappers.PubCrawl;

public static class ChallengeMapper
{
    public static ChallengeDto ToDto(Challenge challenge)
    {
        return new ChallengeDto
        {
            Id = challenge.Id,
            Title = challenge.Title,
            Description = challenge.Description,
            Type = challenge.Type,
            Points = challenge.Points
        };
    }

    public static Challenge ToEntity(CreateChallengeDto dto)
    {
        return new Challenge
        {
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            Points = dto.Points,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateEntity(Challenge challenge, UpdateChallengeDto dto)
    {
        if (dto.Title != null) challenge.Title = dto.Title;
        if (dto.Description != null) challenge.Description = dto.Description;
        if (dto.Type.HasValue) challenge.Type = dto.Type.Value;
        if (dto.Points.HasValue) challenge.Points = dto.Points;
    }
}
