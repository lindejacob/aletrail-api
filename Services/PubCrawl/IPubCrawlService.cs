using aletrail_api.Dtos.PubCrawl;
using aletrail_api.Models;

namespace aletrail_api.Services.PubCrawl;

public interface IPubCrawlService
{
    Task<PubCrawlRouteDto> CreateManualRouteAsync(CreateManualRouteDto dto, int userId);
    Task<PubCrawlRouteDto> GenerateRouteAsync(GenerateRouteDto dto, int userId);
    Task<PubCrawlRouteDto?> GetRouteByIdAsync(int routeId, int userId);
    Task<List<PubCrawlRouteDto>> GetUserRoutesAsync(int userId, int page = 1, int pageSize = 20);
    Task<PubCrawlRouteDto?> UpdateRouteAsync(int routeId, UpdateRouteDto dto, int userId);
    Task<bool> DeleteRouteAsync(int routeId, int userId);
    Task<PubCrawlRouteDto?> AddStopAsync(int routeId, AddStopDto dto, int userId);
    Task<bool> DeleteStopAsync(int routeId, int stopId, int userId);
    Task<PubCrawlRouteDto> JoinRouteAsync(string inviteCode, int userId);
    Task<string> RegenerateInviteCodeAsync(int routeId, int userId);
    Task<bool> RemoveParticipantAsync(int routeId, int participantUserId, int requestingUserId);
}
