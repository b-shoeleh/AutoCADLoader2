using AutoCADLoader.Models.Offices;
using AutoCADLoader.Utility;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADLoader.Models
{
    public class UserInfo
    {
        private Office _office;
        public Office Office
        {
            get
            { return _office; }
            set
            {
                _office = value;
                _office.IsUsersOffice = true;
            }
        }

        public UserInfo()
        {
            // Set up the office and server code based on the office data
            Office userOffice = OfficesCollection.GetUsersLocalOfficeOrDefault();

#if DEBUG
            // For testing a specific office/region
            //Office = "Markham";
            //Region = "CanEast";
#endif

            //userOffice = OfficesCollection.GetOfficeByADNameOrDefault(Office, Region);

            // Fallback values in case all else fails
            userOffice ??= OfficesCollection.GetOfficeByName("Toronto", "CanEast");
            userOffice ??= OfficesCollection.GetFallbackOffice();

            // Add this default information in case the JSON data is incomplete
            if (string.IsNullOrWhiteSpace(userOffice?.Name))
                userOffice.Name = "Toronto";
            if (string.IsNullOrWhiteSpace(userOffice?.OfficeCode))
                userOffice.OfficeCode = "TO";
            if (string.IsNullOrWhiteSpace(userOffice?.ServerCode))
                userOffice.ServerCode = "TO";

            Office = userOffice;
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

        public static string MachineName()
        {
            return System.Environment.MachineName;

        }

        //string machineName = System.Environment.MachineName;
        ////get the 2nd and 3rd characters which are the office code for the machine
        //return machineName.Substring(1, 2);

        public static string DocumentFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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

        public static string LocalAppDataFolder(string subfolder = "")
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            appData = $@"{appData}\{Properties.Settings.Default["CompanyFolder"]}\{Properties.Settings.Default["AppName"]}";

            if (subfolder != "")
            {
                appData = Path.Combine(appData, subfolder);
            }

            return appData;
        }

        public static string CompanyLocalAppDataFolder()
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{Properties.Settings.Default["CompanyFolder"].ToString()}";
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
