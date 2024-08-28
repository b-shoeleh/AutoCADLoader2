using AutoCADLoader.Models.Regions;
using System.Text.Json.Serialization;

namespace AutoCADLoader.Models.Offices
{
    public class Office
    {
        [JsonPropertyName("office")]
        public string Name { get; set; }

        [JsonPropertyName("machineCode")]
        public string OfficeCode { get; set; }

        [JsonPropertyName("serverCode")]
        public string ServerCode { get; set; }

        [JsonPropertyName("region")]
        public string RegionOld { get; set; }

        public bool IsSavedOffice { get; set; } = false;

        public string Id { get; }
        public Region Region { get; }
        public string DirectoryName { get; }
        public string DisplayName { get; }


        public Office(OfficeData data)
        {
            if (string.IsNullOrWhiteSpace(data.OfficeDir))
            {
                throw new ArgumentNullException(nameof(data.OfficeDir));
            }

            if (string.IsNullOrWhiteSpace(data.RegionDir))
            {
                throw new ArgumentNullException(nameof(data.RegionDir));
            }

            Id = $"{data.RegionDir}-{data.OfficeDir}";
            DirectoryName = data.OfficeDir;
            DisplayName = data.DisplayName ?? DirectoryName;
            Region = new Region(data.RegionDir);
        }

        // Used for a UI binding, don't remove for now
        public override string ToString()
        {
            return $"{Region.DisplayName.ToUpper()} - {DisplayName}";
        }
    }
}
