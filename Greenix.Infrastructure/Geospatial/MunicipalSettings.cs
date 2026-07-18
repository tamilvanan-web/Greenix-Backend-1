namespace Greenix.Infrastructure.Geospatial
{
    public class MunicipalSettings
    {
        public const string SectionName = "MunicipalSettings";
        public string CityName { get; set; } = string.Empty;
        public decimal MinLatitude { get; set; }
        public decimal MaxLatitude { get; set; }
        public decimal MinLongitude { get; set; }
        public decimal MaxLongitude { get; set; }
    }
}
