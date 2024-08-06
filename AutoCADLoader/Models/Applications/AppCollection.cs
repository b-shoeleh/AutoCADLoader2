using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutoCADLoader.Models.Applications
{
    public static class InfoCollector
    {

        public static AppCollection AppCollector()
        {
            var appCollection = new AppCollection();

            var fileLocation = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), "VersionInfo.xml");


            if (!File.Exists(fileLocation))
            {
                return appCollection;
            }

            //read version info 
            var AppSerializer = new XmlSerializer(typeof(AppCollection));


            // To read the file, create a FileStream.
            using (Stream fStream = new FileStream(fileLocation, FileMode.Open))
            {
                appCollection = (AppCollection)AppSerializer.Deserialize(fStream);
            }


            //detect what is installed
            if (appCollection != null && appCollection.Apps != null && appCollection.Apps.Count() > 0)
            {
                foreach (AutodeskApplication item in appCollection.Apps)
                {
                    foreach (AppVersion appVersion in item.AppVersions)
                    {
                        appVersion.Installed = RegistryInfo.KeyExists(appVersion.RegKey);

                        if (appVersion.Installed)
                        {
                            //  writeLog($"{item.Title} {appVersion.Number} Detected");

                            if (appVersion.Plugins != null && appVersion.Plugins.Count() > 0)
                            {
                                foreach (var plugin in appVersion.Plugins)
                                {
                                    plugin.Installed = RegistryInfo.KeyExists(plugin.Pluginkey);

                                    //if (plugin.Installed)
                                    //    writeLog($"{item.Title} {appVersion.Number} {plugin.Title} Detected");
                                }
                            }

                        }
                    }
                }
            }



            return appCollection;

        }

        public static PackageCollection PackageCollector()
        {
            var fileLocation = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), "Packages.xml");

            var packageCollection = new PackageCollection();
            var packageSerializer = new XmlSerializer(typeof(PackageCollection));
            using (Stream fStream = new FileStream(fileLocation, FileMode.Open))
            {
                packageCollection = (PackageCollection)packageSerializer.Deserialize(fStream);
            }

            //check to see if the package is already active
            foreach (var package in packageCollection.Packages)
            {
                //check to see if folder exists
                if (Directory.Exists(package.PackageDestPath))
                {
                    package.Active = true;
                }
            }

            return packageCollection;
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
