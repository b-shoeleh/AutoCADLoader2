using AutoCADLoader.Utils;
using System.IO;

namespace AutoCADLoader.Properties
{
    public static class LoaderSettings
    {
        public static string CompanyFolder { get; } = "Arcadis";
        public static string ApplicationName { get; } = "AutoCAD Loader";

        public static int DirectoryAccessTimeout { get; } = 20;

        public static bool RegistryInjection { get; set; } = true;

        public static string LocationsCentralDirectory { get; }
        private const string _centralDirectoryLocationFallback = @"I:\_TechSTND\_Arcadis\AutoCAD";


        static LoaderSettings()
        {
            DirectoryAccessTimeout = RegistryFunctions.GetApplicationValue("DirectoryAccessTimeout") as int? ?? DirectoryAccessTimeout;
            EventLogger.Log($"[SETTING] Directory access timeout: {DirectoryAccessTimeout}", System.Diagnostics.EventLogEntryType.Information);

            int? registryInjection = RegistryFunctions.GetApplicationValue("EnableRegistryInjection") as int?;
            if(registryInjection is not null)
            {
                RegistryInjection = Convert.ToBoolean(registryInjection);
            }
            EventLogger.Log($"[SETTING] Registry injection pathing: {RegistryInjection}", System.Diagnostics.EventLogEntryType.Information);

            string centralDirectoryLocationsStr = RegistryFunctions.GetApplicationValue("LocationsCentral") as string ?? _centralDirectoryLocationFallback;
            string[] centralDirectoryLocations = centralDirectoryLocationsStr.Split(';');
            foreach(string centralDirectoryLocation in centralDirectoryLocations)
            {
                bool isAccessible = IOUtils.IsDirectoryAccessible(centralDirectoryLocation);
                if (isAccessible)
                {
                    EventLogger.Log($"[SETTING] Central directory path: {centralDirectoryLocation}", System.Diagnostics.EventLogEntryType.Information);
                    LocationsCentralDirectory = centralDirectoryLocation;
                    return;
                }
                else
                {
                    EventLogger.Log($"Central directory path is not accessible: {centralDirectoryLocation}", System.Diagnostics.EventLogEntryType.Warning);
                }
            }

            EventLogger.Log($"[SETTING] No central directory path found/accessible, defaulting to: {_centralDirectoryLocationFallback}", System.Diagnostics.EventLogEntryType.Warning);
            LocationsCentralDirectory = _centralDirectoryLocationFallback;
        }

        /// <summary>
        /// Dummy method which can be used to force the constructor to call.
        /// </summary>
        public static void Initialize()
        {

        }

        public static string GetCentralDirectoryPath(string subfolder = "")
        {
            string path = LocationsCentralDirectory;

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                return Path.Combine(path, subfolder);
            }

            return path;
        }

        public static string GetLocalCommonFolderPath(string subfolder = "")
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), CompanyFolder, ApplicationName);

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                path = Path.Combine(path, subfolder);
            }

            return path;
        }

        public static string GetLocalUserFolderPath(string subfolder = "")
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CompanyFolder, ApplicationName);

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                path = Path.Combine(path, subfolder);
            }

            return path;
        }
    }
}
