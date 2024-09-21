using Microsoft.Xaml.Behaviors.Media;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace AutoCADLoader.Models.Applications
{
    // TODO: Utilise these classes properly
    public class AutodeskApplication
    {
        public string Id
        {
            get
            {
                string id = $"{Title}_{Version}";
                if(Plugin is not null)
                {
                    id = string.Concat(id, $"_{Plugin}");
                }

                return id;
            }
        }

        public string AnalysisId { get; set; }

        public string Title { get; set; }

        public VersionYear Version { get; }

        public Plugin? Plugin { get; }

        public bool IsSaved { get; set; } = false;


        public AutodeskApplication(AutodeskApplicationPoco applicationPoco, AppVersionPoco versionPoco, Plugin? plugin = null)
        {
            if (string.IsNullOrWhiteSpace(applicationPoco.AnalysisId))
            {
                throw new ArgumentNullException(nameof(applicationPoco.AnalysisId));
            }

            if (string.IsNullOrWhiteSpace(applicationPoco.Title))
            {
                throw new ArgumentNullException(nameof(applicationPoco.Title));
            }

            AnalysisId = applicationPoco.AnalysisId;
            Title = applicationPoco.Title;

            Version = new(versionPoco);
            Plugin = plugin;
        }


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

        public override string ToString()
        {
            return $"{Title} {Version}";
        }
    }


    public class VersionYear
    {
        public string Id { get; set; }

        public int Number { get; set; }

        public string PlotterVersion { get; set; }

        public string Code { get; set; }

        public string AcadPath { get; set; }

        public string RegKey { get; set; }

        public bool Installed { get; set; }


        public VersionYear(AppVersionPoco data)
        {
            Id = data.Id;
            Number = data.Number;
            PlotterVersion = data.PlotterVersion;
            Code = data.Code;
            AcadPath = data.AcadPath;
            RegKey = data.RegKey;
        }


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

        public override string ToString()
        {
            return Number.ToString();
        }

    }
}
