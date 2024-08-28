namespace AutoCADLoader.Models.Regions
{
    public class Region
    {
        public string DirectoryName { get; }

        public string DisplayName { get; }


        public Region(string regionCode)
        {
            if (string.IsNullOrWhiteSpace(regionCode))
            {
                throw new ArgumentNullException(nameof(regionCode));
            }

            DirectoryName = regionCode;

            if (RegionDefinitions.Definitions.TryGetValue(DirectoryName, out string? value))
            {
                DisplayName = value;
            }
            else
            {
                DisplayName = DirectoryName;
            }
        }
    }
}
