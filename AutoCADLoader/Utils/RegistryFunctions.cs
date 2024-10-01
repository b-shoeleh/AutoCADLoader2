using Microsoft.Win32;

namespace AutoCADLoader.Utils
{
    public static class RegistryFunctions
    {
        // Default registry locations, will be overridden by application settings if they exist

        private static readonly RegistryView _defaultRegistryView = RegistryView.Registry64;

        private static readonly RegistryHive _applicationHive = RegistryHive.LocalMachine;
        private static readonly string _applicationPath = @"SOFTWARE\Arcadis\AutoCAD Loader";
        private static readonly string _installPathKey = "Path";
        private static readonly string _urlApiOfficesKey = "UrlApiOffices";


        static RegistryFunctions()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryPath))
            {
                _applicationPath = Properties.Settings.Default.RegistryPath;
            }

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryKeyInstallPath))
            {
                _installPathKey = Properties.Settings.Default.RegistryKeyInstallPath;
            }

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryKeyOfficesApi))
            {
                _urlApiOfficesKey = Properties.Settings.Default.RegistryKeyOfficesApi;
            }
        }

        public static object? GetApplicationValue(string keyName)
        {
            return Get(keyName, _applicationPath, _applicationHive);
        }

        public static string? GetInstallPath()
        {
            return GetApplicationValue(_installPathKey) as string;
        }

        public static string? GetOfficeApiUrl()
        {
            return GetApplicationValue(_urlApiOfficesKey) as string;
        }

        /// <summary>
        /// Get a string value from the specified registry key.
        /// </summary>
        /// <param name="name">The name of the registry key the value should be retrieved from.</param>
        /// <param name="path">The registry path the value should be retrieved from, not including the hive type.</param>
        /// <param name="hive">The registry hive the value should be retrieved from, e.g. HKLM or HKCU.</param>
        /// <param name="options">Any relevant options for handling the registry value (optional).</param>
        /// <returns>A string with the value if found and successfully parsed, otherwise null.</returns>
        public static object? Get(string name, string path, RegistryHive hive, RegistryValueOptions options = RegistryValueOptions.None)
        {
            path = ValidPathOrDefault(path);
            path = StripHiveTypeFromPath(path); // Strip hive from the path, in case accidentally included

            try
            {
                using (RegistryKey? baseKey = RegistryKey.OpenBaseKey(hive, _defaultRegistryView))
                {
                    using (var subKey = baseKey.OpenSubKey(path))
                    {
                        object? keyValue = subKey?.GetValue(name, null, options);
                        EventLogger.Log($@"Retrieved from registry key in ({hive}): {path}\{name}{Environment.NewLine}{keyValue}",
                            System.Diagnostics.EventLogEntryType.Information);

                        return keyValue;
                    }
                }
            }
            catch
            {
                EventLogger.Log($"Error attempting to access registry key in ({hive}): {path}", System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }

        public static bool Set(
            object keyValue,
            RegistryValueKind registryValueType,
            string keyName,
            string path,
            RegistryHive hive)
        {
            path = ValidPathOrDefault(path);
            path = StripHiveTypeFromPath(path);

            try
            {
                using (RegistryKey? baseKey = RegistryKey.OpenBaseKey(hive, _defaultRegistryView))
                {
                    using (var subKey = baseKey.OpenSubKey(path, true))
                    {
                        if (subKey is null)
                        {
                            using (var newKey = baseKey.CreateSubKey(path))
                            {
                                newKey.SetValue(keyName, keyValue, registryValueType);
                                EventLogger.Log(
                                    $@"Created registry key in ({hive}): {path}\{keyName}
                                    {keyValue}",
                                System.Diagnostics.EventLogEntryType.Information);
                            }
                        }
                        else
                        {
                            subKey.SetValue(keyName, keyValue, registryValueType);
                            EventLogger.Log(
                                $@"Updated registry key in ({hive}): {path}\{keyName}
                                {keyValue}",
                            System.Diagnostics.EventLogEntryType.Information);
                        }
                    }
                }

                return true;
            }
            catch
            {
                EventLogger.Log($@"Error attempting to create/set registry key in ({hive}): {path}\{keyName}", System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        /// <summary>
        /// Get a string value from the 64 bit HKLM hive.
        /// </summary>
        /// <param name="hklmSubKey">Subkey to look for within HKLM</param>
        /// <returns>String value of specified key or empty string if not found.</returns>
        public static string? GetStringFromHklmx64(string valueKeyName, string hklmSubKey)
        {
            hklmSubKey = ValidPathOrDefault(hklmSubKey);

            string? regValue = null;

            RegistryKey? registryKey = null;
            try
            {
                registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            }
            catch
            {
                EventLogger.Log($"Could not access HKLM hive.", System.Diagnostics.EventLogEntryType.Error);
                registryKey?.Close();
                return null;
            }

            if (registryKey is null)
            {
                EventLogger.Log($"Could not access HKLM hive.", System.Diagnostics.EventLogEntryType.Error);
                registryKey?.Close();
                return null;
            }

            try
            {
                registryKey = registryKey.OpenSubKey(hklmSubKey);

                if (registryKey is null)
                {
                    EventLogger.Log($"{hklmSubKey} not found.", System.Diagnostics.EventLogEntryType.Warning);
                    return null;
                }

                try
                {
                    object? regValueKey = registryKey.GetValue(valueKeyName);
                    if (regValueKey is null)
                    {
                        EventLogger.Log($"{hklmSubKey} does not exist.", System.Diagnostics.EventLogEntryType.Warning);
                        registryKey.Close();
                        return null;
                    }

                    string? regValueStr = registryKey.GetValue(valueKeyName) as string;
                    if (regValueStr is null)
                    {
                        EventLogger.Log($"{hklmSubKey} does not have a value.", System.Diagnostics.EventLogEntryType.Warning);
                    }

                    registryKey?.Close();
                    return regValueStr;
                }
                catch
                {
                    EventLogger.Log($"{hklmSubKey} does not have a value.", System.Diagnostics.EventLogEntryType.Warning);
                    registryKey?.Close();
                    return null;
                }
            }
            catch
            {
                EventLogger.Log($"Undefined - could not get HKLM registry value {hklmSubKey}", System.Diagnostics.EventLogEntryType.Error);
                registryKey?.Close();
            }

            return regValue;
        }


        /// <summary>
        /// Checks a passed registry path for validity.
        /// </summary>
        /// <param name="registryPath">Registry path to be checked</param>
        /// <returns>The passed registry path if it passes checks, else the default registry path.</returns>
        private static string ValidPathOrDefault(string registryPath)
        {
            if (!string.IsNullOrWhiteSpace(registryPath))
            {
                return registryPath;
            }

            if (string.IsNullOrWhiteSpace(_applicationPath))
            {
                EventLogger.Log("A valid default registry path for settings has not been set.", System.Diagnostics.EventLogEntryType.Error);
            }

            return _applicationPath;
        }

        private static RegistryHive? GetHiveType(string registryPath)
        {
            if (registryPath.StartsWith("HKEY_LOCAL_MACHINE") || registryPath.StartsWith("HKLM"))
            {
                return RegistryHive.LocalMachine;
            }

            if (registryPath.StartsWith("HKEY_CURRENT_USER") || registryPath.StartsWith("HKCU"))
            {
                return RegistryHive.CurrentUser;
            }

            return null;
        }

        private static string StripHiveTypeFromPath(string path)
        {
            if (path.StartsWith("HKEY_LOCAL_MACHINE"))
            {
                return path.Replace("HKEY_LOCAL_MACHINE\\", string.Empty);
            }

            if (path.StartsWith("HKLM"))
            {
                return path.Replace("HKLM\\", string.Empty);
            }

            if (path.StartsWith("HKEY_CURRENT_USER"))
            {
                return path.Replace("HKEY_CURRENT_USER\\", string.Empty);
            }

            if (path.StartsWith("HKCU"))
            {
                return path.Replace("HKCU\\", string.Empty);
            }

            return path;
        }
    }

}
