using AutoCADLoader.Models.Offices;
using AutoCADLoader.Properties;
using Microsoft.Win32;
using System.IO;

namespace AutoCADLoader.Models
{
    // TODO: Could be better using DI rather than static class
    public static class UserInfo
    {
        private static Office? _savedOffice = null;
        public static Office? SavedOffice
        {
            get
            { return _savedOffice; }
            set
            {
                _savedOffice = value;
                if(_savedOffice is not null)
                {
                    _savedOffice.IsSavedOffice = true;
                }
            }
        }

        static UserInfo()
        {
            SavedOffice = Offices.Offices.GetSavedOfficeOrDefault();
        }

        /// <summary>
        /// Dummy method to force constructor call.
        /// </summary>
        public static void Initialize()
        {

        }

        /// <summary>
        /// get's the user info from the documents
        /// </summary>
        public static string getUserInfo(string domain)
        {
            return Environment.UserName + "@" + domain;
        }

        public static string UserName()
        {
            return Environment.UserName;
        }

        public static string AppDataFolder(string subfolder = "")
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (!string.IsNullOrEmpty(subfolder))
                appData = Path.Combine(appData, subfolder);

            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);


            return appData;
        }

        public static string LocalAppDataFolder(string subfolder = "", bool expanded = true)
        {
            string appData;
            if(expanded)
            {
                appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), LoaderSettings.CompanyFolder, LoaderSettings.ApplicationName);
            }
            else
            {
                appData = Path.Combine("%localappdata%", LoaderSettings.CompanyFolder, LoaderSettings.ApplicationName);
            }

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                appData = Path.Combine(appData, subfolder);
            }

            return appData;
        }

        public static string CompanyLocalAppDataFolder()
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{Properties.Settings.Default["CompanyFolder"]}";
        }



        public static string MachineType()
        {
            switch (System.Environment.MachineName.Substring(0, 1).ToUpper())
            {
                case "D":
                    return "Desktop";
                case "N":
                    return "Notebook";
                case "T":
                    return "Tablet";
                case "V":
                    return "Virtual";

                default:
                    return "Unknown";

            }

        }


        public static bool GetDotNetVersion()
        {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
