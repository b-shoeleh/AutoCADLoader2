using AutoCADLoader.Models.Packages;
using AutoCADLoader.Properties;
using AutoCADLoader.Utils;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace AutoCADLoader.Models.Applications
{
    public static class InfoCollector
    {
        private static AppCollection? _autodeskApplicationsCollection;
        public static AppCollection AutodeskApplicationsCollection
        {
            get
            {
                if (_autodeskApplicationsCollection is null)
                {
                    _autodeskApplicationsCollection = new();
                    PopulateApplicationData();
                }

                return _autodeskApplicationsCollection;
            }
        }

        private static BundleCollection? _bundlesCollection;
        public static BundleCollection BundleCollection
        {
            get
            {
                if (_bundlesCollection is null)
                {
                    _bundlesCollection = new();
                    PopulateBundlesData();
                }

                return _bundlesCollection;
            }
        }


        /// <returns>True if the application definition file was found and serialized (even if there no applications are defined), otherwise false.</returns>
        public static bool PopulateApplicationData()
        {
            // Find the application definitions file
            var fileLocation = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), "VersionInfo.xml");
            if (!File.Exists(fileLocation))
            {
                fileLocation = Path.Combine(LoaderSettings.GetLocalCommonFolderPath("Settings"), "VersionInfo.xml");
                if (!File.Exists(fileLocation))
                {
                    EventLogger.Log("Application definitions file could not be found.", EventLogEntryType.Error);
                    return false;
                }
            }

            //read version info 
            var AppSerializer = new XmlSerializer(typeof(AppCollection));
            using (Stream fStream = new FileStream(fileLocation, FileMode.Open))
            {
                object? applicationDefinitionsFromFile = AppSerializer.Deserialize(fStream);
                if (applicationDefinitionsFromFile is null)
                {
                    EventLogger.Log("Application definitions could not be serialized from application definition file.", EventLogEntryType.Error);
                    return false;
                }

                try
                {
                    _autodeskApplicationsCollection = (AppCollection)applicationDefinitionsFromFile;
                }
                catch
                {
                    EventLogger.Log("Error populating Autodesk applications from application definition file.", EventLogEntryType.Error);
                    return false;
                }
            }

            //detect what is installed
            if (_autodeskApplicationsCollection.Apps.Any())
            {
                foreach (AutodeskApplication item in _autodeskApplicationsCollection.Apps)
                {
                    foreach (AppVersion appVersion in item.AppVersions)
                    {
                        appVersion.Installed = RegistryInfo.KeyExists(appVersion.RegKey);

                        if (appVersion.Installed)
                        {
                            EventLogger.Log($"Application detected - {item.Title} {appVersion.Number}", EventLogEntryType.Information);

                            if (appVersion.Plugins.Any())
                            {
                                foreach (var plugin in appVersion.Plugins)
                                {
                                    plugin.Installed = RegistryInfo.KeyExists(plugin.Pluginkey);
                                    if (plugin.Installed)
                                    {
                                        EventLogger.Log($"Plugin detected - {item.Title} {appVersion.Number} {plugin.Title}", EventLogEntryType.Information);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                EventLogger.Log("No installed applications were detected.", EventLogEntryType.Warning);
            }

            return true; // Applications may not be detected, but the definitions were still populated successfully.
        }

        /// <returns>True if the packages definition file was found and serialized (even if there no packages are defined), otherwise false.</returns>
        public static bool PopulateBundlesData()
        {
            var fileLocation = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), "Packages.xml");

            var packageSerializer = new XmlSerializer(typeof(BundleCollection));
            using (Stream fStream = new FileStream(fileLocation, FileMode.Open))
            {
                object? packagesDefinitonsFromFile = packageSerializer.Deserialize(fStream);
                if (packagesDefinitonsFromFile is null)
                {
                    EventLogger.Log("Package definitions could not be serialized from packages definition file.", EventLogEntryType.Error);
                    return false;
                }

                try
                {
                    _bundlesCollection = (BundleCollection)packagesDefinitonsFromFile;
                }
                catch
                {
                    EventLogger.Log("Error populating packages from packages definition file.", EventLogEntryType.Error);
                    return false;
                }
            }

            if(_bundlesCollection.Packages.Any())
            {
                //check to see if the package is already active
                foreach (var bundle in _bundlesCollection.Packages)
                {
                    //check to see if folder exists
                    if (Directory.Exists(bundle.TargetFilePath))
                    {
                        bundle.Active = true;
                    }
                }
            }
            else
            {
                EventLogger.Log("No installed applications were detected.", EventLogEntryType.Warning);
            }

            return true; // Applications may not be detected, but the definitions were still populated successfully.
        }
    }





    [Serializable()]
    [System.Xml.Serialization.XmlRoot("AppCollection")]
    public class AppCollection
    {
        [XmlArray("Apps")]
        [XmlArrayItem("App", typeof(AutodeskApplication))]
        public AutodeskApplication[] Apps { get; set; }
    }

    [Serializable()]
    public class AutodeskApplication
    {
        [System.Xml.Serialization.XmlElementAttribute("Title")]
        public string Title { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("AnalysisId")]
        public string AnalysisId { get; set; }

        [XmlArray("AppVersions")]
        [XmlArrayItem("AppVersion", typeof(AppVersion))]
        public AppVersion[] AppVersions { get; set; }


        public string GetArguments()
        {
            string arguments = "/product ";

            if (Title == "AutoCAD")
            {
                arguments += "ACAD";
            }
            else if (Title == "Civil3d")
            {
                arguments += "C3D";
            }

            arguments += " /language \"en-US\"";

            return arguments;
        }

    }


    [Serializable()]
    public class AppVersion
    {
        [System.Xml.Serialization.XmlElement("Number")]
        public int Number { get; set; }

        [System.Xml.Serialization.XmlElement("PlotterVersion")]
        public string PlotterVersion { get; set; }

        [System.Xml.Serialization.XmlElement("Id")]
        public string Id { get; set; }

        [System.Xml.Serialization.XmlElement("Code")]
        public string Code { get; set; }

        [System.Xml.Serialization.XmlElement("AcadPath")]
        public string AcadPath { get; set; }

        [System.Xml.Serialization.XmlElement("RegKey")]
        public string RegKey { get; set; }

        [XmlArray("Plugins")]
        [XmlArrayItem("Plugin", typeof(Plugin))]

        public Plugin[] Plugins { get; set; }

        public bool Installed { get; set; }


        public string RunPath
        {
            get
            {
                if (AcadPath.LastIndexOf('\\') == AcadPath.Length)
                {
                    return AcadPath + "acad.exe";
                }
                else
                {
                    return AcadPath + "\\acad.exe";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>The plotter version for this application if specified by the XML (e.g. if the 2020 plot folder should be used for 2022), otherwise defaults to the application number.</returns>
        public string GetPlotterVersionOrDefault()
        {
            if (string.IsNullOrWhiteSpace(PlotterVersion))
            {
                return Number.ToString();
            }

            return PlotterVersion;
        }

    }


    public class Plugin
    {
        public string Title { get; set; }
        public string Pluginkey { get; set; }
        public string Scr { get; set; }
        public string Ld { get; set; }
        public bool Installed { get; set; }

        public string GetArguments()
        {
            string arguments = "";

            if (!string.IsNullOrEmpty(Scr))
                arguments += " /b \"" + Scr + "\"";

            if (!string.IsNullOrEmpty(Ld))
                arguments += " /ld \"" + Ld + "\"";

            return arguments + " ";
        }
    }
}
