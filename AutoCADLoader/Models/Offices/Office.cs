using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoCADLoader.Models.Offices
{
    public class Office
    {
        [JsonPropertyName("office")]
        public string Name { get; set; }

        [JsonPropertyName("nameAD")]
        public string NameAD { get; set; }

        [JsonPropertyName("nameNetworkDrive")]
        public string NameNetworkDrive { get; set; }

        [JsonPropertyName("machineCode")]
        public string OfficeCode { get; set; }

        [JsonPropertyName("serverCode")]
        public string ServerCode { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        public bool IsUsersOffice { get; set; } = false;


        public Office()
        {
            OfficeCode = "NA";
            ServerCode = "NA";
            Name = "NA";
            Region = "NA";
        }

        public string OfficeRegionName
        {
            get
            {
                return (Region + "|" + Name + "|" + OfficeCode).ToUpper();
            }
        }

        public string OfficeNameCode
        {
            get
            {
                return Name + "|" + OfficeCode;
            }
        }

        public string RegionOfficeCode
        {
            get
            {
                return $"{Region} - {Name} ({OfficeCode})";
            }
        }

        /// <returns>String representing the DT server office name corresponding with the Active Directory office name. If empty/not found, defaults to "Toronto".</returns>
        public string GetDTServerOfficeNameByADNameOrDefault()
        {
            if (string.IsNullOrWhiteSpace(NameAD))
            {
                if (string.IsNullOrWhiteSpace(Name)) return "Toronto";
                else
                {
                    switch (Name) // TODO: Remove all these exception cases once "NameAD" is built in to the server JSON
                    {
                        case "Toronto West":
                            {
                                return "Toronto-West";
                            }
                        default:
                            {
                                return Name;
                            }
                    }
                }
            }
            else
                return NameAD;
        }

        /// <summary>
        /// Returns the name of this office within the standards folders - e.g. I:\_TechSTND\acad2020\Plot Styles\europe contains "LondonUK" which does not match the Active Directory office name of just "London"
        /// </summary>
        /// <returns>Corresponding name of this office within the network drive standards folders.</returns>
        public string GetNetworkDriveOfficeNameOrDefault()
        {
            if (string.IsNullOrWhiteSpace(NameNetworkDrive))
            {
                if (string.IsNullOrWhiteSpace(Name)) return "Toronto.55";
                else
                {
                    switch (Name) // TODO: Remove all these exception cases once "NameNetworkDrive" is built in to the server JSON
                    {
                        case "Toronto":
                            {
                                return "Toronto.55";
                            }
                        case "Boca Raton":
                            {
                                return "BocaRaton";
                            }
                        case "London":
                            {
                                if (string.Equals(Region, "CanEast", StringComparison.InvariantCultureIgnoreCase))
                                    return "LondonON";
                                if (string.Equals(Region, "Europe", StringComparison.InvariantCultureIgnoreCase))
                                    return "LondonUK";

                                return Name;
                            }
                        default:
                            {
                                return Name;
                            }
                    }
                }
            }
            else return Name;
        }

        public string iDrivePath
        {
            get
            {
                return Path.Combine(@"\\", Region + ".ibigroup.com\\i\\", OfficeCode);
            }
        }

        public string jDrivePath
        {
            get
            {
                return Path.Combine(@"\\", Region + ".ibigroup.com\\j\\", OfficeCode);
            }
        }
    }
}
