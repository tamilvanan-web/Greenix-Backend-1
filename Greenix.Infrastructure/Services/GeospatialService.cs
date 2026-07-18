using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Greenix.Application.Interfaces;
using Greenix.Infrastructure.Geospatial;

namespace Greenix.Infrastructure.Services
{
    public class GeospatialService : IGeospatialService
    {
        private readonly MunicipalSettings _municipalSettings;
        private const double EarthRadiusKm = 6371.0;

        public GeospatialService(IOptions<MunicipalSettings> municipalSettings)
        {
            _municipalSettings = municipalSettings.Value;
        }

        public double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            // Convert to radians
            var rLat1 = DegreesToRadians((double)lat1);
            var rLon1 = DegreesToRadians((double)lon1);
            var rLat2 = DegreesToRadians((double)lat2);
            var rLon2 = DegreesToRadians((double)lon2);

            // Differences
            var dLat = rLat2 - rLat1;
            var dLon = rLon2 - rLon1;

            // Haversine formula math
            var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(rLat1) * Math.Cos(rLat2) *
                    Math.Pow(Math.Sin(dLon / 2), 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        public bool IsWithinMunicipalBoundaries(decimal latitude, decimal longitude)
        {
            return latitude >= _municipalSettings.MinLatitude &&
                   latitude <= _municipalSettings.MaxLatitude &&
                   longitude >= _municipalSettings.MinLongitude &&
                   longitude <= _municipalSettings.MaxLongitude;
        }

        public Task<string> GetAddressFromCoordinatesAsync(decimal latitude, decimal longitude)
        {
            if (!IsWithinMunicipalBoundaries(latitude, longitude))
            {
                return Task.FromResult("Location outside city limits");
            }

            // Localized mock reverse geocoder mapping coordinates to Chennai neighborhoods for offline development
            if (latitude > 13.06m && longitude > 80.24m)
            {
                return Task.FromResult($"George Town, {_municipalSettings.CityName}, Tamil Nadu 600001");
            }
            if (latitude < 13.01m && longitude > 80.23m)
            {
                return Task.FromResult($"Adyar, {_municipalSettings.CityName}, Tamil Nadu 600020");
            }
            if (latitude < 13.03m && longitude < 80.23m)
            {
                return Task.FromResult($"Velachery, {_municipalSettings.CityName}, Tamil Nadu 600042");
            }
            
            return Task.FromResult($"T. Nagar, {_municipalSettings.CityName}, Tamil Nadu 600017");
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
