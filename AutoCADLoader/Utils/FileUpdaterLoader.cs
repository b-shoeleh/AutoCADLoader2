using AutoCADLoader.Models;
using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Models.Packages;
using AutoCADLoader.Properties;
using AutoCADLoader.Windows;
using System.IO;
using System.Windows;

namespace AutoCADLoader.Utils
{
    public class FileUpdaterLoader(Office rememberedOffice)
    {
        // Index of files, update types and statuses
        public List<FileItem> AllFiles { get; set; } = [];

        private bool isProcessRunning { get; set; }
        public Office RememberedOffice { get; set; } = rememberedOffice;
        public bool UpdatesAvailable
        {
            get
            {
                return GetFilesToUpdate().Any();
            }
        }

        public string CompareAll(bool local = false)
        {
            string sourceDirectoryPath;
            if (local)
            {
                sourceDirectoryPath = LoaderSettings.GetLocalCommonFolderPath();
            }
            else
            {
                sourceDirectoryPath = LoaderSettings.GetCentralDirectoryPath();
            }

            if (!Directory.Exists(sourceDirectoryPath))
            {
                EventLogger.Log($"Error updating AutoCAD Loader files - source not found: {sourceDirectoryPath}", System.Diagnostics.EventLogEntryType.Error);
                return "Update standards folder not found!";
            }

            try // To guard against issues with access permissions
            {
                CompareSettings(sourceDirectoryPath);

                if (local)
                {
                    sourceDirectoryPath = Path.Combine(sourceDirectoryPath, "Cache");
                }
                ComparePackages(sourceDirectoryPath);
            }
            catch
            {
                EventLogger.Log($"Undefined error updating AutoCAD Loader files: {sourceDirectoryPath}", System.Diagnostics.EventLogEntryType.Error);
                return $"Error updating from: {sourceDirectoryPath}";
            }

            EventLogger.Log($"Successfully updated AutoCAD Loader files from: {sourceDirectoryPath}", System.Diagnostics.EventLogEntryType.Information);
            return $"Successfully updated from: {sourceDirectoryPath}";
        }

        private bool ComparePackages(string sourcePath)
        {
            DirectoryInfo source = new(Path.Combine(sourcePath, "Packages"));
            DirectoryInfo target = new(LoaderSettings.GetLocalUserFolderPath(@"Cache\Packages"));

            return CompareFolder(source, target, ResourceType.Package);
        }

        private void CompareSettings(string updatePath)
        {
            DirectoryInfo source = new(Path.Combine(updatePath, "Settings"));
            DirectoryInfo target = new(UserInfo.LocalAppDataFolder("Settings"));

            CompareFolder(source, target, ResourceType.Setting);
        }

        private DateTime ConvertToDate(string sDate, int buffer)
        {

            DateTime tempDate;
            var success = DateTime.TryParse(sDate, out tempDate);

            if (!success)
            {
                tempDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            if (buffer == 1)
            {
                tempDate = tempDate.AddDays(1).AddMinutes(-1);
            }

            return tempDate;

        }

        public int GetFileCount(ResourceType resourceType)
        {
            if (AllFiles != null)
            {
                return AllFiles.Where(f => f.ResourceType == resourceType).Count();
            }
            else
                return 0;
        }

        // Compare the folders and index their types and statuses (existing/updated)
        public bool CompareFolder(DirectoryInfo source, DirectoryInfo target, ResourceType resourceType)
        {
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            FileInfo[] directoryFiles;
            try
            {
                directoryFiles = source.GetFiles();
            }
            catch
            {
                return false;
            }

            foreach (FileInfo fileSrc in directoryFiles)
            {
                string targFile = Path.Combine(target.FullName, fileSrc.Name);
                CompareFile(fileSrc, targFile, resourceType);

            }

            // recursion on sub folders
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = new DirectoryInfo(target.FullName + "\\" + diSourceSubDir.Name);
                CompareFolder(diSourceSubDir, nextTargetSubDir, resourceType);
            }

            return true;
        }


        public void CompareFile(FileInfo fileSrc, string targFile, ResourceType resourceType)
        {
            if (fileSrc == null || !fileSrc.Exists)
                return; // Skip file because it does not exist/cannot be accessed

            DateTime srcLastModified = fileSrc.LastWriteTime;

            FileItem fileItem = new()
            {
                SourceFile = fileSrc,
                DestinFile = targFile,
                ResourceType = resourceType
            };

            String extension = targFile.Substring(targFile.Length - 3, 3).ToLower();
            if (File.Exists(targFile))
            {
                //get the file info
                FileInfo ft = new(targFile);
                DateTime targLastModified = ft.LastWriteTime;

                try
                {
                    if (targLastModified == srcLastModified)
                    {
                        fileItem.Status = Status.Matches;
                    }
                    else if (targLastModified > srcLastModified)
                    {
                        fileItem.Status = Status.Newer;
                    }
                    else
                    {
                        fileItem.Status = Status.Outdated;
                    }

                    AllFiles.Add(fileItem);
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                fileItem.Status = Status.DoesntExist;
                AllFiles.Add(fileItem);
            }
        }

        public IEnumerable<FileItem> GetFilesToUpdate(ResourceType? resourceType = null)
        {
            if (resourceType is null)
            {
                var test = AllFiles.Where(f => f.Status != Status.Matches && f.Status != Status.Newer).ToList();
                return test;
            }
            else
            {
                return AllFiles.Where(f => f.ResourceType == resourceType && f.Status != Status.Matches && f.Status != Status.Newer).ToList();
            }
        }

        public string FileCount(ResourceType resourceType)
        {
            var resourceFiles = AllFiles.Where(f => f.ResourceType == resourceType);
            var existingsFiles = GetFilesToUpdate(resourceType);

            return existingsFiles.Count().ToString() + " / " + resourceFiles.Count().ToString();
        }

        public void Update()
        {
            if (isProcessRunning)
            {
                MessageBox.Show("File update process is already running.");
                return;
            }

            var filesToSync = AllFiles.Where(f => f.Status != Status.Matches && f.Status != Status.Newer).ToList();

            isProcessRunning = true;
            int counter = 0;

            foreach (FileItem item in filesToSync)
            {
                counter++;

                //check to see if folder exists
                FileInfo file = new(item.DestinFile);
                if (!Directory.Exists(file.DirectoryName))
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }
                item.SourceFile.CopyTo(item.DestinFile, true);
                item.Status = Status.Matches;
            }

            isProcessRunning = false;
        }

        public void BackgroundUpdate()
        {
            if (isProcessRunning)
            {
                MessageBox.Show("File update process is already running.");
                return;
            }

            //var filesToSync = AllFiles.Where(f => f.Status != Status.Matches).ToList();
            var filesToSync = AllFiles.Where(f => f.Status != Status.Matches && f.Status != Status.Newer).ToList();

            Thread backgroundThread = new(
                   new ThreadStart(() =>
                   {
                       isProcessRunning = true;
                       int counter = 0;

                       foreach (FileItem item in filesToSync)
                       {
                           counter++;

                           //check to see if folder exists
                           FileInfo file = new(item.DestinFile);
                           if (!Directory.Exists(file.DirectoryName))
                           {
                               Directory.CreateDirectory(file.DirectoryName);
                           }
                           item.SourceFile.CopyTo(item.DestinFile, true);
                           item.Status = Status.Matches;
                       }

                       isProcessRunning = false;
                   }
                ));

            backgroundThread.Start();
        }

        public static void PurgeActivePackages(string appDataFolder)
        {
            if (Directory.Exists(appDataFolder))
            {
                foreach (var item in Directory.GetFiles(appDataFolder))
                {
                    if (File.Exists(item))
                    {
                        try
                        {
                            File.Delete(item);
                        }
                        catch
                        {
                            //skip
                        }
                    }
                }
            }
        }

        public static void CopyPackage(Bundle pkg)
        {
            var src = Path.Combine(UserInfo.LocalAppDataFolder(@"Cache\Packages"), pkg.FileName);
            IOUtils.DirectoryCopy(src, pkg.TargetFilePath, true);
        }

        /// <summary>
        /// Update the plotter registry file with the values specified through the Loader, then import it into the application profile registry entries.
        /// </summary>
        /// <param name="selectedOffice">Office that is currently selected in the working office combobox.</param>
        /// <param name="selectedApplication">Application that is currently selected in the applications combobox.</param>
        /// <returns>Civil 3D units selected by the user (if the software has never previously been run). Otherwise, empty string.</returns> // TODO: Rework this
        public static string? UpdateRegistryFile(Office selectedOffice, AutodeskApplication selectedApplication)
        {
            //get registry template file
            string srcPath = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), "Plotter.txt");

            //get destination path
            string dstPath = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), "Plotter.reg");

            File.Copy(srcPath, dstPath, true);

            //get the active profile
            //string? CurrentProfileName = RegistryInfo.CurrentProfileName(runVersion.Id, runVersion.Code);
            string? CurrentProfileName = RegistryInfo.CurrentProfileName(selectedApplication.Version);

            // TODO: Review/improve this
            if (string.IsNullOrWhiteSpace(CurrentProfileName) && string.Equals(selectedApplication.Title, "Civil3d", StringComparison.InvariantCultureIgnoreCase))
            {
                // First time launch for the selected Civil 3D product - deal with unit selection here
                bool? isImperial = new UnitSelectionWindow().ShowDialog();
                string c3dUnits;
                if (isImperial is not null && isImperial == true)
                {
                    c3dUnits = "<<C3D_Imperial>>";
                }
                else
                {
                    c3dUnits = "<<C3D_Metric>>";
                }

                return c3dUnits;
            }
            else
            {
                string content = File.ReadAllText(dstPath);

                string localUserPath = LoaderSettings.GetLocalUserFolderPath();
                localUserPath = localUserPath.Replace(@"\", @"\\");
                content = content.Replace("[localappdata]", localUserPath);

                string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                content = content.Replace("[appdata]", roamingAppData);

                content = content.Replace("[version]", "acad" + selectedApplication.Version.GetPlotterVersionOrDefault());
                content = content.Replace("[RegKey]", selectedApplication.Version.RegKey);
                content = content.Replace("[profile]", CurrentProfileName);

                using (StreamWriter writer = new(dstPath))
                {
                    writer.Write(content);
                    writer.Close();
                }

                //import the registry into the profile
                RegistryInfo.ImportRegistry(dstPath);

                if (string.IsNullOrWhiteSpace(CurrentProfileName))
                {
                    EventLogger.Log($"Autodesk profile could not be found: {CurrentProfileName}", System.Diagnostics.EventLogEntryType.Warning);
                }
                else
                {
                    // Handle the Fonts/Pats directories
                    RegistryInfo.InjectSupportPaths(selectedApplication.Version.RegKey, CurrentProfileName);
                }
            }

            return null;
        }

        public static string ValidateLocalAppData()
        {
            //check to see if local data folders exists and if not, copy the files from programdata (installation folder) to the local appdata

            var userLocalAppData = UserInfo.LocalAppDataFolder();
            var srcProgramData = Utils.ProgramDataLocation();

            var result = "valid";

            //check to see if 
            if (!Directory.Exists(srcProgramData))
            {
                result = "Bad Installation";
            }
            else
            {
                if (!Directory.Exists(userLocalAppData))
                {
                    Directory.CreateDirectory(userLocalAppData);
                }



                if (Directory.GetDirectories(userLocalAppData).Count() == 0)
                {
                    foreach (var folder in Directory.GetDirectories(srcProgramData))
                    {
                        var directoryInfo = new DirectoryInfo(folder);
                        var localDataFolder = userLocalAppData + "\\" + directoryInfo.Name;
                        IOUtils.DirectoryCopy(folder, localDataFolder, true);
                    }
                }
                else
                {
                    foreach (var folder in Directory.GetDirectories(srcProgramData))
                    {

                        var directoryInfo = new DirectoryInfo(folder);
                        var localDataFolder = userLocalAppData + "\\" + directoryInfo.Name;

                        if (Directory.GetFiles(localDataFolder).Length == 0 && Directory.GetDirectories(localDataFolder).Length == 0)
                            IOUtils.DirectoryCopy(folder, localDataFolder, true);
                    }
                }

                result = "Directory synced from install folder!";
            }
            return result;
        }
    }

    public class FileItem
    {
        public FileInfo SourceFile { get; set; }
        public string DestinFile { get; set; }
        public Status Status { get; set; }
        public ResourceType ResourceType { get; set; }
    }
}
