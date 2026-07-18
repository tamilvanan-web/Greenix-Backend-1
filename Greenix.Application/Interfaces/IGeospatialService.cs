using System.Threading.Tasks;

namespace Greenix.Application.Interfaces
{
    public interface IGeospatialService
    {
        double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2);
        bool IsWithinMunicipalBoundaries(decimal latitude, decimal longitude);
        Task<string> GetAddressFromCoordinatesAsync(decimal latitude, decimal longitude);
    }
}
