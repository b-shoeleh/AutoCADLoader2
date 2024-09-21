namespace AutoCADLoader.Models.Applications
{
    public static class AutodeskApplicationsInstalled
    {
        // TODO: Make DI
        // TODO: Implement methods to make this private
        public static List<AutodeskApplication> Data { get; set; } = [];


        static AutodeskApplicationsInstalled()
        {
            
        }

        public static void Initialize(List<AutodeskApplication> autodeskApplications)
        {
            Data = autodeskApplications;
        }


        public static AutodeskApplication? GetByIdOrDefault(string id)
        {
            return Data.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool SetSavedApplication(string name, string version, string? plugin)
        {
            string assembledId = $"{name}_{version}";
            if (!string.IsNullOrWhiteSpace(plugin))
            {
                assembledId = string.Concat(assembledId, $"_{plugin}");
            }

            // Remove any existing applications marked as saved
            IEnumerable<AutodeskApplication>? savedApplications = Data.Where(a => a.IsSaved);
            foreach(AutodeskApplication application in savedApplications)
            {
                application.IsSaved = false;
            }

            // Mark the specified office as saved (if found)
            AutodeskApplication? savedApplication = Data.FirstOrDefault(a => string.Equals(a.Id, assembledId));
            if (savedApplication is not null)
            {
                savedApplication.IsSaved = true;
                return true;
            }

            return false;
        }
    }
}
