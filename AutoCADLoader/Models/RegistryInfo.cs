using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Models.Packages;
using AutoCADLoader.Utils;
using AutoCADLoader.ViewModels;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;

namespace AutoCADLoader.Models
{
    // TODO: Might be better if this was DI instead of static, but this class may be removed eventually anyway
    public static class RegistryInfo
    {
        private readonly static RegistryHive _hive = RegistryHive.CurrentUser;

        static string AutoCADLoaderKeyLocation { get; } = @"Software\Arcadis\AutoCAD Loader";

        public static string? Title { get; set; }
        public static string? Version { get; set; }
        public static string? Plugin { get; set; }

        public static List<string> Bundles { get; set; }

        /// <summary>
        /// Office code for the working office saved to registry when the user last started an application.
        /// </summary>
        public static string? SavedOfficeId { get; set; }

        public static string? Hardware { get; set; }

        public static DateTime? LastUpdated { get; set; }


        static RegistryInfo()
        {
            Bundles = [];
            LoadPreviousSettings();
        }

        /// <summary>
        /// Dummy method to force constructor call.
        /// </summary>
        public static void Initialize()
        {

        }

        /// <returns>Name of the registry subkey where Loader values for the most recently launched CAD application are - e.g. "AutoCAD_2022".</returns>
        public static string GetAppEntryLocation()
        {
            string regPath = $"{Title}_{Version}";

            if (!string.IsNullOrWhiteSpace(Plugin))
            {
                regPath = $"{regPath}_{Plugin}";
            }

            return regPath;
        }

        /// <summary>
        /// delete the registry sub key from windows registry
        /// </summary>
        /// <param name="Root"></param>
        /// <param name="searchKey"></param>
        private static void DeleteSubKeys(RegistryKey Root, string searchKey)
        {

            if (Root == null)
            {
                return;
            }

            foreach (string keyname in Root.GetSubKeyNames())
            {
                try
                {
                    using (RegistryKey key = Root.OpenSubKey(keyname, true))
                    {
                        if (key == null)
                        {
                            return;
                        }

                        if (keyname == searchKey)
                        {
                            try
                            {

                                Root.DeleteSubKeyTree(searchKey);
                                break;

                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        DeleteSubKeys(key, searchKey);
                    }
                }
                catch (System.Security.SecurityException se)
                {

                }
                catch (Exception ex)
                {

                }
            }
        }

        public static void DeleteProfile(string path, string regkey)
        {
            RegistryKey key = Registry.CurrentUser;
            //open subkey
            RegistryKey profile = key.OpenSubKey(path, true);

            if (profile != null)
            {
                try
                {
                    profile.DeleteSubKeyTree(regkey);
                    profile.Close();

                }
                catch
                {
                    //doesn't exist
                }
            }
        }


        public static void ImportRegistry(string filePath)
        {
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "reg.exe";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;

                string command = "import \"" + filePath + "\"";
                proc.StartInfo.Arguments = command;
                proc.Start();

                proc.WaitForExit();
            }
            catch (System.Exception)
            {
                proc.Dispose();
            }
        }

        /// <param name="versionNo">Autodesk product version number (ACADVER), e.g. "R24.1"</param>
        /// <param name="versionCode">Autodesk product version code, e.g. "ACAD-5101:409"</param>
        /// <returns>The registry key path of the Autodesk profile for the Autodesk application specified - e.g. "Software\Autodesk\AutoCAD\R24.1\ACAD-5101:409\Profiles".</returns>
        public static string GetAutodeskProfileKeyLocation(string versionNo, string versionCode)
        {
            return $@"Software\Autodesk\AutoCAD\{versionNo}\{versionCode}\Profiles";
        }


        /// <summary>
        /// looks at the root of that profiles and select what is current, the only key there.
        /// </summary>
        /// <param name="versionNo">Autodesk product version number (ACADVER), e.g. "R24.1"</param>
        /// <param name="versionCode">Autodesk product version code, e.g. "ACAD-5101:409"</param>
        /// <returns>Name of the profile most recently used within the specified Autodesk product, otherwise null.</returns>
        public static string? CurrentProfileName(VersionYear versionYear)
        {
            string regPath = GetAutodeskProfileKeyLocation(versionYear.Id, versionYear.Code);
            try
            {
                RegistryKey? RKCU = Registry.CurrentUser.OpenSubKey(regPath, false);

                // The default (null) key contains the name of the most recently used profile within the Autodesk product
                return RKCU?.GetValue(null) as string;
            }
            catch
            {
                return null;
            }
        }

        public static void LoadPreviousSettings()
        {
            EventLogger.Log("Loading previous settings...", EventLogEntryType.Information);

            try
            {
                EventLogger.Log($"Previous settings registry path: {_hive}, {AutoCADLoaderKeyLocation}", EventLogEntryType.Information);

                Title = RegistryFunctions.Get("Title", AutoCADLoaderKeyLocation, _hive) as string;
                EventLogger.Log($"Previously saved title: {Title}", EventLogEntryType.Information);

                Version = RegistryFunctions.Get("Version", AutoCADLoaderKeyLocation, _hive) as string;
                EventLogger.Log($"Previously saved version: {Version}", EventLogEntryType.Information);

                Plugin = RegistryFunctions.Get("Plugin", AutoCADLoaderKeyLocation, _hive) as string;
                EventLogger.Log($"Previously saved plugin: {Plugin}", EventLogEntryType.Information);

                string? lastUpdated = RegistryFunctions.Get("LastUpdated", AutoCADLoaderKeyLocation, _hive) as string;
                if (!string.IsNullOrWhiteSpace(lastUpdated))
                {
                    DateTime intervalVal;
                    if (DateTime.TryParse(lastUpdated, out intervalVal))
                    {
                        LastUpdated = intervalVal;
                    }
                    else
                    {
                        EventLogger.Log($"Could not parse last updated: {lastUpdated}", EventLogEntryType.Warning);
                    }
                }

                EventLogger.Log($"Last updated: {LastUpdated}", EventLogEntryType.Information);
            }
            catch
            {
                EventLogger.Log("Error retrieving previous user settings from registry.", EventLogEntryType.Error);
            }

            LoadPreviousVersionSettings(Title, Version, Plugin);
        }

        private static void LoadPreviousVersionSettings(string? title, string? version, string? plugin)
        {
            string VersionKeyLocation = $@"{AutoCADLoaderKeyLocation}\{title}_{version}";
            if (!string.IsNullOrWhiteSpace(plugin))
            {
                VersionKeyLocation += $"_{plugin}";
            }
            EventLogger.Log($"Loading previously selected version settings from: {VersionKeyLocation}", EventLogEntryType.Information);

            SavedOfficeId = RegistryFunctions.Get("ActiveOffice", VersionKeyLocation, RegistryHive.CurrentUser) as string;

            string? _packages = RegistryFunctions.Get("Packages", VersionKeyLocation, RegistryHive.CurrentUser) as string;
            Bundles.Clear();
            if (!string.IsNullOrWhiteSpace(_packages))
            {
                Bundles = [.. _packages.Split(',')];
            }

            Hardware = RegistryFunctions.Get("Hardware", VersionKeyLocation, RegistryHive.CurrentUser) as string ?? "true";
        }

        public static void SaveSettings(
            string title,
            string version,
            string? plugin,
            IEnumerable<Bundle>? bundles,
            string activeOffice,
            bool hardwareAccelerationEnabled)
        {
            // TODO: Check whether this assignment is really still needed
            Title = title;
            Version = version;
            Plugin = plugin ?? string.Empty;

            Bundles = [.. bundles?.Select(b => b.Title)];
            SavedOfficeId = activeOffice;
            Hardware = hardwareAccelerationEnabled ? "true" : "false";

            SaveUserRegistry(Title, Version, Plugin);
        }

        /// <summary>
        /// Update the registry values for AutoCAD Loader (to remember settings on next time the Loader is launched).
        /// </summary>
        public static void SaveUserRegistry(string title, string version, string plugin)
        {
            EventLogger.Log($"Saving user settings to: {_hive}, {AutoCADLoaderKeyLocation}", EventLogEntryType.Information);

            try
            {
                RegistryFunctions.Set(title, RegistryValueKind.String, "Title", AutoCADLoaderKeyLocation, _hive);
                RegistryFunctions.Set(version, RegistryValueKind.String, "Version", AutoCADLoaderKeyLocation, _hive);
                RegistryFunctions.Set(plugin, RegistryValueKind.String, "Plugin", AutoCADLoaderKeyLocation, _hive);
            }
            catch
            {
                EventLogger.Log("Error saving user registry settings - cannot set values.", EventLogEntryType.Error);
                MessageBox.Show("Error saving user settings.");
            }

            //update the version key info
            string VersionKeyLocation = $@"{AutoCADLoaderKeyLocation}\{GetAppEntryLocation()}";

            try
            {
                string packageList = string.Join(",", Bundles);
                packageList ??= string.Empty;

                RegistryFunctions.Set(packageList, RegistryValueKind.String, "Packages", VersionKeyLocation, RegistryHive.CurrentUser);
                RegistryFunctions.Set(SavedOfficeId, RegistryValueKind.String, "ActiveOffice", VersionKeyLocation, RegistryHive.CurrentUser);
                RegistryFunctions.Set(Hardware, RegistryValueKind.String, "Hardware", VersionKeyLocation, RegistryHive.CurrentUser);
            }
            catch
            {
                EventLogger.Log("Error saving user registry settings - cannot set values.", EventLogEntryType.Error);
                MessageBox.Show("Error saving user settings.");
            }
        }

        public static void SaveLastUpdated(DateTime lastUpdated)
        {
            try
            {
                RegistryFunctions.Set(lastUpdated, RegistryValueKind.String, "LastUpdated", AutoCADLoaderKeyLocation, _hive);
            }
            catch
            {
                EventLogger.Log("Error saving last updated setting - cannot set values.", EventLogEntryType.Error);
            }
        }

        public static bool KeyExists(string key)
        {
            string regPath = "Software\\" + key;

            try
            {
                RegistryKey RKCU = Registry.LocalMachine.OpenSubKey(regPath, false);

                if (RKCU != null)
                {
                    //key found, so installed
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                return false;

            }

        }

        /// <summary>
        /// Update the plotter file for registry import
        /// </summary>
        /// <returns>Civil 3D unit selection (if found)</returns>
        public static string? UpdateProfilePlotters(AutodeskApplication selectedApplication, Office selectedOffice)
        {
            string units = FileUpdaterLoader.UpdateRegistryFile(selectedOffice, selectedApplication);
            return units;
        }

        // TODO: Consider moving this out into an application class
        public static void InjectSupportPaths(string versionId, string profileName)
        {
            string registryPath = @$"SOFTWARE\{versionId}\Profiles\{profileName}\General";
            string registryValueName = "ACAD";

            // Support path types to add
            string[] pathTypesToAdd = ["Fonts", "Pats"];

            // Get the current support paths within the current AutoCAD profile
            string? supportPaths = RegistryFunctions.Get(registryValueName, registryPath, RegistryHive.CurrentUser, RegistryValueOptions.DoNotExpandEnvironmentNames) as string;

            // Check if the support paths have previously been added - if not, add them
            if (!string.IsNullOrWhiteSpace(supportPaths))
            {
                foreach (string pathType in pathTypesToAdd)
                {
                    string expandedPath = UserInfo.LocalAppDataFolder(pathType);
                    string nonExpandedPath = UserInfo.LocalAppDataFolder(pathType, false);
                    // AutoCAD will re-save non-expanded paths to this format, so this is what needs to be searched for to ensure no duplicates
                    nonExpandedPath = nonExpandedPath.Replace("%localappdata%", @"%UserProfile%\AppData\Local", StringComparison.InvariantCultureIgnoreCase);

                    if (!supportPaths.Contains(expandedPath, StringComparison.InvariantCultureIgnoreCase) && !supportPaths.Contains(nonExpandedPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        supportPaths = string.Concat(supportPaths, nonExpandedPath, ";");
                    }
                }

                RegistryFunctions.Set(supportPaths, RegistryValueKind.ExpandString, registryValueName, registryPath, RegistryHive.CurrentUser);
                EventLogger.Log($"Setting support paths:" +
                    $"{Environment.NewLine}{registryPath}" +
                    $"{Environment.NewLine}{supportPaths}",
                    EventLogEntryType.Information);
            }
        }
    }
}
