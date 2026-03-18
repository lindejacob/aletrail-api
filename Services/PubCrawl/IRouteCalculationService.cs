using aletrail_api.Models;

namespace aletrail_api.Services.PubCrawl;

public interface IRouteCalculationService
{
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    List<Bar> FindNearbyBars(List<Bar> allBars, double centerLat, double centerLon, double radiusKm, int limit);
    List<Bar> OptimizeRoute(List<Bar> bars, double startLat, double startLon, double endLat, double endLon);
    double CalculateTotalDistance(List<Bar> orderedBars, double? startLat = null, double? startLon = null, double? endLat = null, double? endLon = null);
}
