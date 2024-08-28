using System.Text.Json.Serialization;

namespace AutoCADLoader.Models.Offices
{
    /// <summary>
    /// POCO to deserialize office data from.
    /// </summary>
    public class OfficeData
    {
        [JsonPropertyName("regionDir")]
        public string? RegionDir { get; set; }

        [JsonPropertyName("officeDir")]
        public string? OfficeDir { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }
}
