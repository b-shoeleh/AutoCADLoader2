using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADLoader.Utility
{
    public static class RegistryFunctions
    {
        // Default registry locations, will be overridden by application settings if they exist

        private static readonly string _registryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\IBI Group\IBI Group - AutoCAD Loader";

        private static readonly string _installPathKey = "Path";
        private static readonly string _urlOfficeApiKey = "URLOfficeAPI";


        static RegistryFunctions()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryPath))
                _registryPath = Properties.Settings.Default.RegistryPath;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryKeyInstallPath))
                _installPathKey = Properties.Settings.Default.RegistryKeyInstallPath;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.RegistryKeyOfficesApi))
                _urlOfficeApiKey = Properties.Settings.Default.RegistryKeyOfficesApi;
        }

        public static string GetInstallPath()
        {
            return GetString(_installPathKey);
        }

        public static string GetOfficeApiUrl()
        {
            return GetString(_urlOfficeApiKey);
        }


        public static string GetString(string regName, string regPath = null)
        {
            regPath ??= _registryPath;

            string regValue = null;
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
