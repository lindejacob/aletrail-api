using System.Device.Location;
using aletrail_api.Models;
using Humanizer;

namespace aletrail_api.Services.PubCrawl;

public class RouteCalculationService : IRouteCalculationService
{
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var cordinate1 = new GeoCoordinate(lat1, lon1);
        var cordinate2 = new GeoCoordinate(lat2, lon2);
        return cordinate1.GetDistanceTo(cordinate2);
    }

    public double CalculateTotalDistance(List<Bar> orderedBars, double startLat, double startLon, double endLat, double endLon)
    {
        double totalDistance = 0;
        double currentLat = startLat;
        double currentLon = startLon;

        foreach (var bar in orderedBars)
        {
            totalDistance += CalculateDistance(currentLat, currentLon, bar.Latitude, bar.Longitude);
            currentLat = bar.Latitude;
            currentLon = bar.Longitude;
        }

        totalDistance += CalculateDistance(currentLat, currentLon, endLat, endLon);
        return totalDistance;
    }

    public List<Bar> FindNearbyBars(List<Bar> allBars, double centerLat, double centerLon, double radiusKm, int limit)
    {
        double radiusMeters = radiusKm * 1000;

        var nearbyBars = allBars
            .Select(bar => new
            {
                Bar = bar,
                Distance = CalculateDistance(centerLat, centerLon, bar.Latitude, bar.Longitude)
            })
            .Where(x => x.Distance <= radiusMeters)
            .OrderBy(x => x.Distance)
            .Take(limit)
            .Select(x => x.Bar)
            .ToList();

        return nearbyBars;
    }

    public List<Bar> OptimizeRoute(List<Bar> bars, double startLat, double startLon, double endLat, double endLon)
    {
        if (bars.Count == 0) return new List<Bar>();
        if (bars.Count == 1) return new List<Bar>(bars);

        var route = new List<Bar>();
        var unvisited = new List<Bar>(bars);
        double currentLat = startLat;
        double currentLon = startLon;

        while (unvisited.Count > 0)
        {
            Bar nearestBar = unvisited[0];
            double minDistance = CalculateDistance(currentLat, currentLon, nearestBar.Latitude, nearestBar.Longitude);

            foreach (var bar in unvisited)
            {
                double distance = CalculateDistance(currentLat, currentLon, bar.Latitude, bar.Longitude);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestBar = bar;
                }
            }

            route.Add(nearestBar);
            unvisited.Remove(nearestBar);
            currentLat = nearestBar.Latitude;
            currentLon = nearestBar.Longitude;
        }

        return route;
    }
}
