using AutoCADLoader.Models;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Properties;
using System.IO;

namespace AutoCADLoader.Utils
{
    public static class FileSyncManager
    {
        private static bool _enabled = true;
        public static bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                EventLogger.Log($"File synchronisation: {value}", System.Diagnostics.EventLogEntryType.Information);
                _enabled = value;
            }
        }


        /// <summary>
        /// Checks to see if any local user data has been created, and if not, create this by copying from local common data.
        /// </summary>
        /// <returns>True if first run has been determined, otherwise false.</returns>
        public static bool HandleFirstRun()
        {
            string localAppDataPath = UserInfo.LocalAppDataFolder();
            if (!Directory.Exists(localAppDataPath))
            {
                IOUtils.DirectoryCopy(LoaderSettings.GetLocalCommonFolderPath(), localAppDataPath, true);
                return true;
            }

            return false;
        }

        /// <summary>
        /// To be called when critical files are missing, to restore them from Program Data snapshot.
        /// </summary>
        /// <returns></returns>
        public static void ValidateCriticalFiles()
        {
            ValidateSettings();
        }

        private static void ValidateSettings()
        {
            string commonDataPath = LoaderSettings.GetLocalCommonFolderPath("Settings");
            string localAppDataPath = UserInfo.LocalAppDataFolder("Settings");

            bool settingsFilesMissing = IOUtils.AnyFolderDifferenceQuick(commonDataPath, localAppDataPath, false, false);
            if (settingsFilesMissing)
            {
                IOUtils.DirectoryCopy(commonDataPath, localAppDataPath, true);
            }
        }


        /// <summary>
        /// Cache standards for the specified office from the local standards snapshot to the user's local appdata cache (fast).
        /// </summary>
        /// <param name="office">The office for which the standards should be retrieved.</param>
        public static void CacheOfficeFromProgramData(Office office)
        {
            string source = LoaderSettings.GetLocalCommonFolderPath("Cache");
            string target = LoaderSettings.GetLocalUserFolderPath("Cache");

            CacheOfficeStandards(office, source, target);
        }

        /// <summary>
        /// Replicate the standards for the specified office from Azure to the user's local appdata cache (up to date).
        /// </summary>
        /// <param name="office">The office for which the standards should be synchronized.</param>
        public static void SynchronizeFromAzure(Office office)
        {
            if(!Enabled)
            {
                return;
            }

            string source = LoaderSettings.GetCentralDirectoryPath();
            string target = LoaderSettings.GetLocalUserFolderPath("Cache");

            CacheOfficeStandards(office, source, target);
        }

        /// <summary>
        /// Synchronize from the user data folder to AutoCAD pathed folders.
        /// TODO: Can be improved for efficiency
        /// </summary>
        public static bool SynchronizeFromUserData(Office office)
        {
            string sourceBase = LoaderSettings.GetLocalUserFolderPath("Cache");
            string targetBase = LoaderSettings.GetLocalUserFolderPath();

            IEnumerable<(string source, string target)> standardsSubPaths = GetStandardsSubPaths(office);
            foreach (var standardsSubPath in standardsSubPaths)
            {
                // Clear the AutoCAD pathed directory, as we don't want content from other offices to remain
                IOUtils.DirectoryDelete(Path.Combine(targetBase, standardsSubPath.target));

                IOUtils.DirectoryCopy(Path.Combine(sourceBase, standardsSubPath.source), Path.Combine(targetBase, standardsSubPath.target), false);
            }

            return true;
        }


        public static List<(string source, string target)> GetStandardsSubPaths(Office office)
        {
            List<(string source, string target)> standardsSubPaths = [];

            string[] standardsTypesSubpaths = ["Fonts", "Pats", "Packages"];
            foreach (string standardsTypeSubpath in standardsTypesSubpaths)
            {
                standardsSubPaths.Add((standardsTypeSubpath, standardsTypeSubpath));
            }

            string[] _standardsTypesSubpathsRegional = ["PlotStyles", "Plotters"];
            foreach (string standardsTypesSubpath in _standardsTypesSubpathsRegional)
            {
                standardsSubPaths.Add((Path.Combine(standardsTypesSubpath, "_Common"), standardsTypesSubpath));
                standardsSubPaths.Add((Path.Combine(standardsTypesSubpath, office.Region.DirectoryName, "_Common"), standardsTypesSubpath));
                standardsSubPaths.Add((Path.Combine(standardsTypesSubpath, office.Region.DirectoryName, office.DirectoryName), standardsTypesSubpath));
            }

            // PMP directory remains a subfolder in the AutoCAD pathing
            standardsSubPaths.Add((Path.Combine("Plotters", office.Region.DirectoryName, office.DirectoryName, "PMP"), Path.Combine("Plotters", "PMP")));

            return standardsSubPaths;
        }

        /// <summary>
        /// Replicate standards for a specified office to a cache.
        /// </summary>
        /// <param name="office">Specified office for which standards should be cached.</param>
        /// <param name="source">Directory path the standards will be sourced from.</param>
        /// <param name="target">Directory path the standards will be cached to.</param>
        private static void CacheOfficeStandards(Office office, string source, string target)
        {
            var standardsSubpaths = GetStandardsSubPaths(office);

            // Copy all required paths within each source standards directory to the target directory
            foreach (var standardsSubpath in standardsSubpaths)
            {
                {
                    // Replicate the subdirectory structure to the target directory.
                    IOUtils.DirectoryCopy(Path.Combine(source, standardsSubpath.source), Path.Combine(target, standardsSubpath.source), false);
                }
            }
        }
    }
}