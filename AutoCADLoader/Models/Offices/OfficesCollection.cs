using AutoCADLoader.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoCADLoader.Models.Offices
{
    public static class OfficesCollection
    {
        private const string _jsonSubfolderPath = @"Arcadis\AutoCAD Loader";
        private const string _jsonFileName = "offices.json";

        public static List<Office> Data = [];

        //static Offices()
        //{
        //  //  LoadOfficesData();
        //}

        public static string LoadOfficesData()
        {
            string status = null;

            bool officeDataSet = SetOfficesFromServer(RegistryFunctions.GetOfficeApiUrl()); // Attempt to load office data from server
            if (officeDataSet)
            {
                status = "Resources loaded from server";
                EventLogger.Log(status, EventLogEntryType.Information);
            }



            // Fallback methods of loading office data in event of server outage

            if (!officeDataSet) // Attempt to load office data from a local file
            {

                EventLogger.Log("office Data not set, reading from local file instead", EventLogEntryType.Information);

                string installedJsonPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    _jsonSubfolderPath, _jsonFileName);

                string appDataJsonPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    _jsonSubfolderPath, _jsonFileName);

                JSONFunctions.UpdateJson(installedJsonPath, appDataJsonPath); // Check/update from installed version if required
                officeDataSet = SetOfficesFromLocalFile(appDataJsonPath);

                if (officeDataSet)
                    status = "Resources loaded from local user data";

                if (!officeDataSet) // Attempt to load office data from the installed JSON
                {
                    officeDataSet = (SetOfficesFromLocalFile(installedJsonPath));

                    if (officeDataSet)
                    {
                        JSONFunctions.UpdateJson(installedJsonPath, appDataJsonPath, false); // AppData version has failed - overwrite it with the installed version                        
                        status = "Resources loaded from local installed data";
                    }
                }
            }

            if (Data is not null)
            {
                CleanUpOfficeList();
            }

            if (Data is null || !Data.Any())
            {
                status = null;
            }

            return status;
        }


        public static bool SetOfficesFromServer(string url)
        {
            EventLogger.Log("Setting offices from server: " + url, EventLogEntryType.Information);

            string json = JSONFunctions.GetJsonFromServer(url);

            if (string.IsNullOrWhiteSpace(json))
            {
                EventLogger.Log("Json was empty", EventLogEntryType.Information);
                return false;
            }


            EventLogger.Log("Json successfull", EventLogEntryType.Information);

            var result = SetOfficesFromJson(json);

            EventLogger.Log("Json " + result, EventLogEntryType.Information);


            if (SetOfficesFromJson(json).Equals("Success"))
            {
                EventLogger.Log("Offices retrieved from json successfully...", EventLogEntryType.Information);

                string appDataFilepath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                appDataFilepath = Path.Combine(appDataFilepath, _jsonSubfolderPath);
                JSONFunctions.SaveJsonToLocalFile(json, appDataFilepath, _jsonFileName); // Save a copy to appdata, as the server version will be the most up to date copy

                EventLogger.Log("AppDataFilePath : " + appDataFilepath, EventLogEntryType.Information);

                return true;
            }

            return false;
        }

        public static bool SetOfficesFromLocalFile(string localFilepath = null)
        {
            if (string.IsNullOrWhiteSpace(localFilepath))
                return false;

            string json = JSONFunctions.GetJsonFromLocalFile(localFilepath);

            if (string.IsNullOrWhiteSpace(json))
                return false;

            if (SetOfficesFromJson(json).Equals("Success"))
            {
                return true;
            }

            return false;
        }

        public static Office GetOfficeByName(string officeName, string officeRegion)
        {
            Office result = null;
            // Match by office name and region
            result = Data.FirstOrDefault(o =>
            string.Equals(o.Name, officeName, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(o.Region, officeRegion, StringComparison.InvariantCultureIgnoreCase));

            return result;
        }

        /// <summary>
        /// Find the specified Office object or a default Office within the list of all offices, based on the supplied Active Directory office name.
        /// </summary>
        /// <param name="officeADName">Office name as provided by Active Directory</param>
        /// <returns>The office object matching the specified Active Directory office name. Returns Toronto office if not found.</returns>
        public static Office GetOfficeByADNameOrDefault(string officeADName, string officeRegion)
        {
            Office result = null;
            // Match by office name and region
            result = Data.FirstOrDefault(o =>
            string.Equals(o.GetDTServerOfficeNameByADNameOrDefault(), officeADName, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(o.Region, officeRegion, StringComparison.InvariantCultureIgnoreCase));

            return result;
        }

        /// <summary>
        /// Find the specified Office object within the list of all offices, based on the supplied machine code.
        /// </summary>
        /// <param name="machineCode">Two character machine code to find corresponding Office for, e.g. "TO".</param>
        /// <returns>The office object matching the specified machine code. Returns null if not found.</returns>
        public static Office? GetOfficeByMachineCode(string machineCode)
        {
            return Data.FirstOrDefault(o => string.Equals(o.OfficeCode, machineCode));
        }

        /// <summary>
        /// Find the specified Office object or a default Office within the list of all offices, based on the supplied machine code.
        /// </summary>
        /// <param name="machineCode">Two character machine code to find corresponding Office for, e.g. "TO".</param>
        /// <returns>The office object matching the specified machine code. Returns Toronto office if not found.</returns>
        public static Office GetOfficeByMachineCodeOrDefault(string machineCode)
        {
            Office result = GetOfficeByMachineCode(machineCode);

            if (result is null)
            {
                result = Data.FirstOrDefault(o => string.Equals(o.OfficeCode, "TO"));
            }

            return result;
        }

        public static List<Office> GetOfficesByServerCode(string serverCode)
        {
            List<Office> result = Data.Where(o => string.Equals(o.ServerCode, serverCode)).ToList();
            return result;
        }

        public static List<Office> GetOfficesByRegion(string region)
        {
            List<Office> result = Data.Where(o => string.Equals(o.Region, region, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(o => o.Name)
                .ToList();

            return result;
        }

        public static Office GetUsersLocalOfficeOrDefault()
        {
            Office result = Data.FirstOrDefault(o => o.IsUsersOffice);
            result ??= GetOfficeByMachineCode("TO");

            return result;
        }

        /// <returns>Get the office remembered in registry from the user's last selection. If none, returns a default office.</returns>
        public static Office GetRememberedOfficeOrDefault()
        {
            return Data.FirstOrDefault(o => o.IsUsersOffice) ?? GetFallbackOffice();
        }

        /// <summary>
        /// Removes any office that does not have a name/office code provided.
        /// </summary>
        public static void CleanUpOfficeList()
        {
            Data = Data
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .Where(o => !string.IsNullOrWhiteSpace(o.OfficeCode))
                .OrderBy(o => o.Region)
                .ToList();
        }

        private static string SetOfficesFromJson(string json = "")
        {
            EventLogger.Log("Setting offices from json...", EventLogEntryType.Information);

            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return "Application JSON was not valid.";
                }

                Data = JsonSerializer.Deserialize<List<Office>>(json);
                if (Data != null)
                    return "Success";
            }
            catch (JsonException)
            {
                return "JsonException - JSON could not be deserialized into list of applications.";
            }
            catch
            {
                return "Undefined - JSON could not be deserialized into list of applications.";
            }

            return "Unknown error";
        }

        /// <returns>A Toronto office object which is guaranteed not to be null, even if it does not exist in the JSON.</returns>
        public static Office GetFallbackOffice()
        {
            Office fallbackOffice = GetOfficeByMachineCodeOrDefault("TO");

            fallbackOffice ??= new()
            {
                OfficeCode = "TO",
                ServerCode = "TO",
                Name = "Toronto",
                Region = "CanEast",
                IsUsersOffice = false
            };

            return fallbackOffice;
        }
    }
}
