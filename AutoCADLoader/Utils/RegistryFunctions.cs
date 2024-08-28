using Microsoft.Win32;

namespace AutoCADLoader.Utility
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
    }
}
