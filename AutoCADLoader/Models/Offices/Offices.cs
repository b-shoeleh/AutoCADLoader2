using AutoCADLoader.Utility;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace AutoCADLoader.Models.Offices
{
    public static class Offices
    {
        private const string _jsonSubfolderPath = @"Arcadis\AutoCAD Loader\Settings";
        private const string _jsonFileName = "offices.json";

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static List<Office> Data = [];


        public static string? LoadOfficesData()
        {
            string? status = null;

            // Attempt to load office data from server
            bool officeDataSet = SetOfficesFromServer(RegistryFunctions.GetOfficeApiUrl());

            string installedJsonPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                _jsonSubfolderPath, _jsonFileName);
            string appDataJsonPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                _jsonSubfolderPath, _jsonFileName);

            if (officeDataSet)
            {
                status = "Offices successfully loaded from API.";
                EventLogger.Log(status, EventLogEntryType.Information);
            }

            // Fallback methods of loading office data in event of server outage
            if (!officeDataSet)
            {
                EventLogger.Log("Offices data not set, falling back to local data file.", EventLogEntryType.Warning);

                JSONFunctions.UpdateJson(installedJsonPath, appDataJsonPath); // Check/update from installed version if required
                officeDataSet = SetOfficesFromLocalFile(appDataJsonPath);

                if (officeDataSet)
                {
                    status = "Resources loaded from local user data";
                    EventLogger.Log(status, EventLogEntryType.Information);
                }

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

            if (Data is null || Data.Count == 0)
            {
                return null;
            }

            Data = [.. Data.OrderBy(o => o.Region.DisplayName).ThenBy(o => o.DisplayName)];

            return status;
        }


        public static bool SetOfficesFromServer(string url)
        {
            EventLogger.Log("Setting offices from server: " + url, EventLogEntryType.Information);

            string? json = JSONFunctions.GetJsonFromServer(url);

            if (string.IsNullOrWhiteSpace(json))
            {
                EventLogger.Log("JSON was empty", EventLogEntryType.Information);
                return false;
            }


            EventLogger.Log("JSON successful", EventLogEntryType.Information);

            var result = SetOfficesFromJson(json);

            EventLogger.Log("JSON " + result, EventLogEntryType.Information);


            if (SetOfficesFromJson(json).Equals("Success"))
            {
                EventLogger.Log("Offices retrieved from json successfully.", EventLogEntryType.Information);

                string appDataFilepath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                appDataFilepath = Path.Combine(appDataFilepath, _jsonSubfolderPath);
                JSONFunctions.SaveJsonToLocalFile(json, appDataFilepath, _jsonFileName); // Save a copy to appdata, as the server version will be the most up to date copy

                EventLogger.Log("AppDataFilePath: " + appDataFilepath, EventLogEntryType.Information);

                return true;
            }

            return false;
        }

        public static bool SetOfficesFromLocalFile(string localFilepath = "")
        {
            if (string.IsNullOrWhiteSpace(localFilepath))
            {
                return false;
            }

            string json = JSONFunctions.GetJsonFromLocalFile(localFilepath);

            if (string.IsNullOrWhiteSpace(json))
                return false;

            if (SetOfficesFromJson(json).Equals("Success"))
            {
                return true;
            }

            return false;
        }

        public static void SetRememberedOffice(Office office)
        {
            List<Office> rememberedOffices = [.. Data.Where(o => o.IsSavedOffice)];

            foreach (Office rememberedOffice in rememberedOffices)
            {
                rememberedOffice.IsSavedOffice = false;
            }

            office.IsSavedOffice = true;
            UserInfo.SavedOffice = office;
        }

        public static Office GetOfficeByName(string officeName, string? officeRegion = null)
        {
            Office? result = null;

            // Match by office name and region
            result = Data.FirstOrDefault(o =>
                string.Equals(o.DirectoryName, officeName, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(o.Region.DirectoryName, officeRegion, StringComparison.InvariantCultureIgnoreCase));

            return result;
        }

        /// <summary>
        /// Find the specified Office object within the list of all offices, based on the supplied office ID.
        /// </summary>
        /// <param name="officeId">Unique identifier for the office.</param>
        /// <returns>The office object matching the specified ID. Returns null if not found.</returns>
        public static Office? GetOfficeById(string officeId)
        {
            return Data.FirstOrDefault(o => string.Equals(o.Id, officeId));
        }

        /// <summary>
        /// Find the specified Office object or a default Office within the list of all offices, based on the supplied ID.
        /// </summary>
        /// <param name="officeId">Two character machine code to find corresponding Office for, e.g. "TO".</param>
        /// <returns>The office object matching the specified machine code. Returns Toronto office if not found.</returns>
        public static Office GetOfficeByIdOrDefault(string officeId)
        {
            Office? result = GetOfficeById(officeId);
            result ??= GetFallbackOffice();
            return result;
        }

        public static List<Office> GetOfficesByServerCode(string serverCode)
        {
            List<Office> result = Data.Where(o => string.Equals(o.ServerCode, serverCode)).ToList();
            return result;
        }

        public static List<Office> GetOfficesByRegion(string region)
        {
            List<Office> result = [..Data
                .Where(o => string.Equals(o.Region.DisplayName, region, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(o => o.ToString())];

            return result;
        }

        /// <returns>Get the office remembered in registry from the user's last selection. If none, returns a default office.</returns>
        public static Office GetSavedOfficeOrDefault()
        {
            return Data.FirstOrDefault(o => o.IsSavedOffice) ?? GetFallbackOffice();
        }


        private static string SetOfficesFromJson(string json = "")
        {
            EventLogger.Log("Setting offices from JSON...", EventLogEntryType.Information);

            if (string.IsNullOrWhiteSpace(json))
            {
                return "Application JSON was not valid.";
            }

            try
            {
                var serializedData = JsonSerializer.Deserialize<List<OfficeData>>(json, jsonOptions);
                if (serializedData is not null)
                {
                    Data.Clear();
                    foreach (var item in serializedData)
                    {
                        Office officeToAdd;
                        try
                        {
                            officeToAdd = new(item);
                        }
                        catch (ArgumentNullException ex)
                        {
                            EventLogger.Log($"Office data not sufficient to add office: {item.OfficeDir}, {ex.ParamName}", EventLogEntryType.Warning);
                            continue;
                        }

                        Data.Add(officeToAdd);
                    }

                    return "Success";
                }
            }
            catch (JsonException)
            {
                EventLogger.Log("JsonException - JSON could not be deserialized into list of applications", EventLogEntryType.Error);
                return "JsonException - JSON could not be deserialized into list of applications.";
            }
            catch (Exception ex)
            {
                EventLogger.Log("Undefined - JSON could not be deserialized into list of applications", EventLogEntryType.Error);
                return "Undefined - JSON could not be deserialized into list of applications.";
            }

            return "Unknown error";
        }

        /// <returns>A Toronto office object which is guaranteed not to be null, even if it does not exist in the JSON.</returns>
        public static Office GetFallbackOffice()
        {
            Office? fallbackOffice = GetOfficeById("ACA-Toronto55");
            fallbackOffice ??= new(new OfficeData { RegionDir = "ACA", OfficeDir = "Toronto55", DisplayName = "Toronto" });

            return fallbackOffice;
        }
    }
}
