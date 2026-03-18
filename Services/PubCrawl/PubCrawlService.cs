using aletrail_api.DAL;
using aletrail_api.Dtos.PubCrawl;
using aletrail_api.Models;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.Services.PubCrawl;

public class PubCrawlService : IPubCrawlService
{
    private readonly ApplicationDbContext _context;
    private readonly IRouteCalculationService _routeCalc;

    public PubCrawlService(ApplicationDbContext context, IRouteCalculationService routeCalc)
    {
        _context = context;
        _routeCalc = routeCalc;
    }

    public async Task<PubCrawlRouteDto> CreateManualRouteAsync(CreateManualRouteDto dto, int userId)
    {
        var route = new PubCrawlRoute
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            InviteCode = GenerateInviteCode()
        };

        _context.PubCrawlRoutes.Add(route);
        await _context.SaveChangesAsync();

        var barIds = dto.Stops.Select(s => s.BarOsmId).ToList();
        var bars = await _context.Bars
            .Where(b => barIds.Contains(b.OsmId))
            .ToDictionaryAsync(b => b.OsmId);

        foreach (var stopDto in dto.Stops.OrderBy(s => s.OrderIndex))
        {
            if (!bars.ContainsKey(stopDto.BarOsmId))
                throw new Exception($"Bar with OSM ID {stopDto.BarOsmId} not found");

            var stop = new RouteStop
            {
                PubCrawlRouteId = route.Id,
                BarOsmId = stopDto.BarOsmId,
                OrderIndex = stopDto.OrderIndex,
                Notes = stopDto.Notes,
                ChallengeId = stopDto.ChallengeId
            };

            _context.RouteStops.Add(stop);
        }

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(route.Id, userId) 
            ?? throw new Exception("Failed to retrieve created route");
    }

    public async Task<PubCrawlRouteDto> GenerateRouteAsync(GenerateRouteDto dto, int userId)
    {
        double centerLat = (dto.StartLatitude + dto.EndLatitude) / 2;
        double centerLon = (dto.StartLongitude + dto.EndLongitude) / 2;

        var allBars = await _context.Bars.ToListAsync();
        
        double distance = _routeCalc.CalculateDistance(
            dto.StartLatitude, dto.StartLongitude,
            dto.EndLatitude, dto.EndLongitude
        );
        double searchRadius = Math.Max(distance * 0.75, 5.0);
        
        var candidateBars = _routeCalc.FindNearbyBars(
            allBars, 
            centerLat, 
            centerLon, 
            searchRadius, 
            Math.Min(dto.NumberOfBars * 3, 100)
        );

        if (candidateBars.Count < dto.NumberOfBars)
        {
            throw new Exception($"Not enough bars found. Found {candidateBars.Count}, needed {dto.NumberOfBars}");
        }

        var selectedBars = candidateBars.Take(dto.NumberOfBars).ToList();
        
        var optimizedBars = _routeCalc.OptimizeRoute(
            selectedBars,
            dto.StartLatitude,
            dto.StartLongitude,
            dto.EndLatitude,
            dto.EndLongitude
        );

        var route = new PubCrawlRoute
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            InviteCode = GenerateInviteCode()
        };

        _context.PubCrawlRoutes.Add(route);
        await _context.SaveChangesAsync();

        for (int i = 0; i < optimizedBars.Count; i++)
        {
            var stop = new RouteStop
            {
                PubCrawlRouteId = route.Id,
                BarOsmId = optimizedBars[i].OsmId,
                OrderIndex = i,
                Notes = null
            };

            _context.RouteStops.Add(stop);
        }

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(route.Id, userId)
            ?? throw new Exception("Failed to retrieve generated route");
    }

    public async Task<PubCrawlRouteDto?> GetRouteByIdAsync(int routeId, int userId)
    {
        var route = await _context.PubCrawlRoutes
            .Include(r => r.CreatedBy)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Bar)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Challenge)
            .Include(r => r.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(r => r.Id == routeId);

        if (route == null) return null;

        bool isOwner = route.CreatedByUserId == userId;
        bool isParticipant = route.Participants.Any(p => p.UserId == userId);

        if (!isOwner && !isParticipant)
            return null;

        var orderedStops = route.Stops.OrderBy(s => s.OrderIndex).ToList();
        var bars = orderedStops.Select(s => s.Bar).ToList();
        var totalDistance = _routeCalc.CalculateTotalDistance(bars);

        return new PubCrawlRouteDto
        {
            Id = route.Id,
            Name = route.Name,
            Description = route.Description,
            CreatedByUserId = route.CreatedByUserId,
            CreatedByUsername = route.CreatedBy.Username,
            InviteCode = isOwner ? route.InviteCode : null,
            CreatedAt = route.CreatedAt,
            UpdatedAt = route.UpdatedAt,
            TotalDistanceKm = totalDistance,
            IsOwner = isOwner,
            Participants = route.Participants.Select(p => new ParticipantDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Username = p.User.Username,
                JoinedAt = p.JoinedAt
            }).ToList(),
            Stops = orderedStops.Select(s => new RouteStopDto
            {
                Id = s.Id,
                BarOsmId = s.BarOsmId,
                BarName = s.Bar.Name,
                BarLatitude = s.Bar.Latitude,
                BarLongitude = s.Bar.Longitude,
                OrderIndex = s.OrderIndex,
                Notes = s.Notes,
                Challenge = s.Challenge != null ? new ChallengeDto
                {
                    Id = s.Challenge.Id,
                    Title = s.Challenge.Title,
                    Description = s.Challenge.Description,
                    Type = s.Challenge.Type,
                    Points = s.Challenge.Points
                } : null
            }).ToList()
        };
    }

    public async Task<List<PubCrawlRouteDto>> GetUserRoutesAsync(int userId, int page = 1, int pageSize = 20)
    {
        var ownedRoutes = _context.PubCrawlRoutes
            .Where(r => r.CreatedByUserId == userId);
        
        var participantRoutes = _context.PubCrawlRoutes
            .Where(r => r.Participants.Any(p => p.UserId == userId));

        var query = ownedRoutes.Union(participantRoutes)
            .Include(r => r.CreatedBy)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Bar)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Challenge)
            .Include(r => r.Participants)
                .ThenInclude(p => p.User);

        var routes = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return routes.Select(route =>
        {
            bool isOwner = route.CreatedByUserId == userId;
            var orderedStops = route.Stops.OrderBy(s => s.OrderIndex).ToList();
            var bars = orderedStops.Select(s => s.Bar).ToList();
            var totalDistance = _routeCalc.CalculateTotalDistance(bars);

            return new PubCrawlRouteDto
            {
                Id = route.Id,
                Name = route.Name,
                Description = route.Description,
                CreatedByUserId = route.CreatedByUserId,
                CreatedByUsername = route.CreatedBy.Username,
                InviteCode = isOwner ? route.InviteCode : null,
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt,
                TotalDistanceKm = totalDistance,
                IsOwner = isOwner,
                Participants = route.Participants.Select(p => new ParticipantDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    JoinedAt = p.JoinedAt
                }).ToList(),
                Stops = orderedStops.Select(s => new RouteStopDto
                {
                    Id = s.Id,
                    BarOsmId = s.BarOsmId,
                    BarName = s.Bar.Name,
                    BarLatitude = s.Bar.Latitude,
                    BarLongitude = s.Bar.Longitude,
                    OrderIndex = s.OrderIndex,
                    Notes = s.Notes,
                    Challenge = s.Challenge != null ? new ChallengeDto
                    {
                        Id = s.Challenge.Id,
                        Title = s.Challenge.Title,
                        Description = s.Challenge.Description,
                        Type = s.Challenge.Type,
                        Points = s.Challenge.Points
                    } : null
                }).ToList()
            };
        }).ToList();
    }

    public async Task<PubCrawlRouteDto?> UpdateRouteAsync(int routeId, UpdateRouteDto dto, int userId)
    {
        var route = await _context.PubCrawlRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.CreatedByUserId == userId);

        if (route == null) return null;

        if (dto.Name != null) route.Name = dto.Name;
        if (dto.Description != null) route.Description = dto.Description;
        
        route.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(routeId, userId);
    }

    public async Task<bool> DeleteRouteAsync(int routeId, int userId)
    {
        var route = await _context.PubCrawlRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.CreatedByUserId == userId);

        if (route == null) return false;

        _context.PubCrawlRoutes.Remove(route);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PubCrawlRouteDto?> AddStopAsync(int routeId, AddStopDto dto, int userId)
    {
        var route = await _context.PubCrawlRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.Id == routeId && r.CreatedByUserId == userId);

        if (route == null) return null;

        var bar = await _context.Bars.FirstOrDefaultAsync(b => b.OsmId == dto.BarOsmId);
        if (bar == null)
            throw new Exception($"Bar with OSM ID {dto.BarOsmId} not found");

        int orderIndex = dto.OrderIndex ?? route.Stops.Count;
        
        var existingStops = await _context.RouteStops
            .Where(s => s.PubCrawlRouteId == routeId && s.OrderIndex >= orderIndex)
            .ToListAsync();

        foreach (var stop in existingStops)
        {
            stop.OrderIndex++;
        }

        var newStop = new RouteStop
        {
            PubCrawlRouteId = routeId,
            BarOsmId = dto.BarOsmId,
            OrderIndex = orderIndex,
            Notes = dto.Notes,
            ChallengeId = dto.ChallengeId
        };

        _context.RouteStops.Add(newStop);
        route.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(routeId, userId);
    }

    public async Task<bool> DeleteStopAsync(int routeId, int stopId, int userId)
    {
        var route = await _context.PubCrawlRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.Id == routeId && r.CreatedByUserId == userId);

        if (route == null) return false;

        var stop = await _context.RouteStops
            .FirstOrDefaultAsync(s => s.Id == stopId && s.PubCrawlRouteId == routeId);

        if (stop == null) return false;

        int deletedOrderIndex = stop.OrderIndex;
        
        _context.RouteStops.Remove(stop);

        var stopsToReorder = await _context.RouteStops
            .Where(s => s.PubCrawlRouteId == routeId && s.OrderIndex > deletedOrderIndex)
            .ToListAsync();

        foreach (var s in stopsToReorder)
        {
            s.OrderIndex--;
        }

        route.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PubCrawlRouteDto> JoinRouteAsync(string inviteCode, int userId)
    {
        var route = await _context.PubCrawlRoutes
            .Include(r => r.Participants)
            .FirstOrDefaultAsync(r => r.InviteCode == inviteCode);

        if (route == null)
            throw new Exception("Invalid invite code");

        if (route.CreatedByUserId == userId)
            throw new Exception("You are the owner of this route");

        if (route.Participants.Any(p => p.UserId == userId))
            throw new Exception("You are already a participant");

        var participant = new PubCrawlParticipant
        {
            PubCrawlRouteId = route.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _context.PubCrawlParticipants.Add(participant);
        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(route.Id, userId)
            ?? throw new Exception("Failed to retrieve route after joining");
    }

    public async Task<string> RegenerateInviteCodeAsync(int routeId, int userId)
    {
        var route = await _context.PubCrawlRoutes
            .FirstOrDefaultAsync(r => r.Id == routeId && r.CreatedByUserId == userId);

        if (route == null)
            throw new Exception("Route not found or access denied");

        route.InviteCode = GenerateInviteCode();
        route.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return route.InviteCode;
    }

    public async Task<bool> RemoveParticipantAsync(int routeId, int participantUserId, int requestingUserId)
    {
        var route = await _context.PubCrawlRoutes
            .Include(r => r.Participants)
            .FirstOrDefaultAsync(r => r.Id == routeId);

        if (route == null) return false;

        bool isOwner = route.CreatedByUserId == requestingUserId;
        bool isSelf = participantUserId == requestingUserId;

        if (!isOwner && !isSelf)
            return false;

        var participant = route.Participants
            .FirstOrDefault(p => p.UserId == participantUserId);

        if (participant == null) return false;

        _context.PubCrawlParticipants.Remove(participant);
        await _context.SaveChangesAsync();

        return true;
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
