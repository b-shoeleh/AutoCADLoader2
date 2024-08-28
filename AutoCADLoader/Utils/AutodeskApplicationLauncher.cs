using AutoCADLoader.Models;
using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Models.Packages;
using AutoCADLoader.Utility;
using AutoCADLoader.ViewModels;
using System.Diagnostics;
using System.IO;

namespace AutoCADLoader.Utils
{
    public static class AutodeskApplicationLauncher
    {
        public static void Launch(InstalledAutodeskApplication selectedApplication,
            IEnumerable<Bundle> bundles,
            Office selectedOffice,
            bool resetAllSettings, bool hardwareAcceleration)
        {
            RegistryInfo.Title = selectedApplication.AutodeskApplication.Title;
            RegistryInfo.Version = selectedApplication.AppVersion.Number.ToString();

            string? civil3dUnits = RegistryInfo.UpdateProfilePlotters(selectedApplication, selectedOffice);

            RegistryInfo.ActiveOffice = selectedOffice.OfficeCode;
            RegistryInfo.Plugin = selectedApplication.Plugin?.Title ?? string.Empty;

            // Handle bundles
            foreach (Bundle bundle in bundles)
            {
                if (bundle.Active)
                {
                    FileUpdaterLoader.CopyPackage(bundle);
                    bundle.Active = true;
                    RegistryInfo.Bundles.Add(bundle.Title); // TODO: Review, don't think this is needed any more
                }
                else
                {
                    // Remove from the destination folder to disable it
                    IOUtils.DirectoryDelete(bundle.TargetFilePath);
                    bundle.Active = false;
                }
            }

            if (File.Exists(selectedApplication.AppVersion.RunPath))
            {
                Process autoCADProcess = new();
                autoCADProcess.StartInfo.FileName = selectedApplication.AppVersion.RunPath;
                autoCADProcess.StartInfo.WorkingDirectory = selectedApplication.AppVersion.AcadPath;
                string appArguments = selectedApplication.AutodeskApplication.GetArguments();

                if (selectedApplication.Plugin is not null)
                {
                    string pluginArguments = selectedApplication.Plugin.GetArguments();
                    appArguments += pluginArguments;
                    RegistryInfo.Plugin = selectedApplication.Plugin.Title;
                }
                else
                {
                    RegistryInfo.Plugin = string.Empty;
                }

                if (hardwareAcceleration)
                {
                    RegistryInfo.Hardware = "true";
                }
                else
                {
                    appArguments += " /nohardware";
                    RegistryInfo.Hardware = "false";
                }

                // TODO: Review - not sure if this works in newer versions of AutoCAD
                if (resetAllSettings)
                {
                    appArguments += " /reset";
                }

                if (!string.IsNullOrWhiteSpace(civil3dUnits) && selectedApplication.AutodeskApplication.Title == "Civil3d")
                {
                    appArguments += " /p " + civil3dUnits;
                }

                EventLogger.Log("Launching with arguments:" +
                    $"{Environment.NewLine}{appArguments}",
                    EventLogEntryType.Information);

                autoCADProcess.StartInfo.Arguments = appArguments;
                autoCADProcess.Start();
            }
        }
    }
}
