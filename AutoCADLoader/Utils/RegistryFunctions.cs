using Microsoft.Win32;

namespace AutoCADLoader.Utils
{
    public static class RegistryFunctions
    {
        // Default registry locations, will be overridden by application settings if they exist

        private static readonly string _registryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Arcadis\AutoCAD Loader";

        private static readonly string _installPathKey = "Path";
        private static readonly string _urlApiOfficesKey = "UrlApiOffices";


        static RegistryFunctions()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryPath))
                _registryPath = Properties.Settings.Default.RegistryPath;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryKeyInstallPath))
                _installPathKey = Properties.Settings.Default.RegistryKeyInstallPath;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryKeyOfficesApi))
                _urlApiOfficesKey = Properties.Settings.Default.RegistryKeyOfficesApi;
        }

        public static string GetInstallPath()
        {
            return GetString(_installPathKey);
        }

        public static string GetOfficeApiUrl()
        {
            return GetString(_urlApiOfficesKey);
        }


        public static string? GetString(string regName, string regPath = null)
        {
            regPath ??= _registryPath;

            string regValue;
            try
            {
                regValue = (string)Registry.GetValue(regPath, regName, null);
            }
            catch
            {
                return null;
            }

            return regValue;
        }

        // TODO: Improve these registry methods for future use

        public static string? GetStringWithOptions(string regName, string regPath = "", RegistryValueOptions options = RegistryValueOptions.None)
        {
            if(string.IsNullOrWhiteSpace(regPath))
            {
                regPath = _registryPath;
            }

            bool hklm = false;
            if (regPath.StartsWith("HKEY_LOCAL_MACHINE"))
            {
                hklm = true;
                regPath = regPath.Replace("HKEY_LOCAL_MACHINE\\", string.Empty);
            }
            else if (regPath.StartsWith("HKLM"))
            {
                hklm = true;
                regPath = regPath.Replace("HKLM\\", string.Empty);
            }
            else if (regPath.StartsWith("HKEY_CURRENT_USER"))
            {
                hklm = false;
                regPath = regPath.Replace("HKEY_CURRENT_USER\\", string.Empty);
            }
            else if (regPath.StartsWith("HKCU"))
            {
                hklm = false;
                regPath = regPath.Replace("HKCU\\", string.Empty);
            }
            else
            {
                EventLogger.Log($"Attempting to access registry key outside HKLM or HKCU: {regPath}", System.Diagnostics.EventLogEntryType.Error);
                return null;
            }

            try
            {
                using (RegistryKey? registryKey = hklm ? Registry.LocalMachine.OpenSubKey(regPath) : Registry.CurrentUser.OpenSubKey(regPath))
                {
                    if (registryKey is null)
                    {
                        return null;
                    }

                    regPath ??= _registryPath;

                    string? regValue = registryKey.GetValue(regName, null, options) as string;
                    return regValue;
                }
            }
            catch
            {
                EventLogger.Log($"Error attempting to access registry key: {(hklm ? "HKLM" : "HKCU")}\\{regPath}", System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }

        // TODO: Look at genericising this
        public static bool Set(string regName, string regValue, string regPath = "", RegistryValueKind registryValueType = RegistryValueKind.String)
        {
            if (string.IsNullOrWhiteSpace(regPath))
            {
                regPath = _registryPath;
            }

            bool hklm = false;
            if (regPath.StartsWith("HKEY_LOCAL_MACHINE"))
            {
                hklm = true;
                regPath = regPath.Replace("HKEY_LOCAL_MACHINE\\", string.Empty);
            }
            else if (regPath.StartsWith("HKLM"))
            {
                hklm = true;
                regPath = regPath.Replace("HKLM\\", string.Empty);
            }
            else if (regPath.StartsWith("HKEY_CURRENT_USER"))
            {
                hklm = false;
                regPath = regPath.Replace("HKEY_CURRENT_USER\\", string.Empty);
            }
            else if (regPath.StartsWith("HKCU"))
            {
                hklm = false;
                regPath = regPath.Replace("HKCU\\", string.Empty);
            }
            else
            {
                EventLogger.Log($"Attempting to access registry key outside HKLM or HKCU: {regPath}", System.Diagnostics.EventLogEntryType.Error);
                return false;
            }

            try
            {
                using (RegistryKey? registryKey = hklm ? Registry.LocalMachine.OpenSubKey(regPath, true) : Registry.CurrentUser.OpenSubKey(regPath, true))
                {
                    if (registryKey is null)
                    {
                        return false;
                    }

                    registryKey.SetValue(regName, regValue, registryValueType);
                    return true;
                }
            }
            catch
            {
                EventLogger.Log($"Error attempting to access registry key: {(hklm ? "HKLM" : "HKCU")}\\{regPath}", System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        public static int? GetIntFromDword(string regName, string? regPath = null)
        {
            regPath = ValidPathOrDefault(regPath);

            int? regValue;
            try
            {
                regValue = Registry.GetValue(regPath, regName, null) as int?;
                if (regValue is not null)
                    try
                    {
                        return regValue;
                    }
                    catch
                    {
                        EventLogger.Log($"Undefined - could not parse {regName} value to Boolean. Default value will be used.", System.Diagnostics.EventLogEntryType.Error);
                        return null;
                    }

                EventLogger.Log($"Registry entry for {regName} not found. Default value will be used.", System.Diagnostics.EventLogEntryType.Warning);
                return null;
            }
            catch
            {
                EventLogger.Log($"Undefined - could not get bool from registry value {regName}.", System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }

        /// <summary>
        /// Checks a passed registry path for validity.
        /// </summary>
        /// <param name="regPath">Registry path to be checked</param>
        /// <returns>The passed registry path if it passes checks, else the default registry path.</returns>
        private static string ValidPathOrDefault(string? regPath)
        {
            if (!string.IsNullOrWhiteSpace(regPath))
                return regPath;

            if (string.IsNullOrWhiteSpace(_registryPath))
            {
                EventLogger.Log("A valid default registry path for settings has not been set.", System.Diagnostics.EventLogEntryType.Error);
            }

            return _registryPath;
        }
    }
}
