using AutoCADLoader.Models.Applications;
using AutoCADLoader.Models.Offices;
using AutoCADLoader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Linq.Expressions;

namespace AutoCADLoader.Utility
{
    public class FileUpdater(Office rememberedOffice)
    {
        public string UpdateFromPath { get; set; } = @"I:";
        public List<FileItem> AllFiles { get; set; } = [];
        private bool isProcessRunning { get; set; }
        public Office RememberedOffice { get; set; } = rememberedOffice;

        public string CompareAll()
        {
            if (!Directory.Exists(UpdateFromPath))
            {
                //abort update
                return "Update standards folder not found!";
            }

            try // To guard against issues with access permissions
            {
                ComparePackages();
                CompareXML();
                CompareSupport();
            }
            catch
            {
                MessageBox.Show(
                    "Failed to update standards content.",
                    "Error updating",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return $"Error updating from: {UpdateFromPath}";
            }

            return $"Successfully updated from: {UpdateFromPath}";
        }


        public bool ComparePackages()
        {
            var source = new DirectoryInfo(Utils.NetPackageFolder(UpdateFromPath));
            var target = new DirectoryInfo(UserInfo.LocalAppDataFolder("Packages"));

            return CompareFolder(source, target, ResourceType.Package);
        }


        public void CompareXML()
        {
            var source = new DirectoryInfo(Utils.NetXMLFolder(UpdateFromPath));
            var target = new DirectoryInfo(UserInfo.LocalAppDataFolder("Settings"));

            //compare all source xml files
            CompareFolder(source, target, ResourceType.Setting);

            //compare office.csv file
            string officeCode = "OfficeCodes.csv";
            var officeSrc = new FileInfo(Path.Combine(Utils.NetRefFolder(UpdateFromPath), officeCode));
            var officeDest = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), officeCode);
            CompareFile(officeSrc, officeDest, ResourceType.Support);

        }

        public void CompareSupport()
        {
            var source = new DirectoryInfo(Utils.NetSupportFolder(UpdateFromPath));
            var target = new DirectoryInfo(Utils.SrcSupportLocation());

            CompareFolder(source, target, ResourceType.Support);
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

            FileItem fileItem = new FileItem();
            fileItem.SourceFile = fileSrc;
            fileItem.DestinFile = targFile;
            fileItem.ResourceType = resourceType;

            String extension = targFile.Substring(targFile.Length - 3, 3).ToLower();
            if (File.Exists(targFile))
            {
                //get the file info
                FileInfo ft = new FileInfo(targFile);
                DateTime targLastModified = ft.LastWriteTime;

                try
                {
                    if (srcLastModified != targLastModified)
                    {
                        fileItem.Status = Status.Updated;
                        AllFiles.Add(fileItem);
                    }
                    else
                    {
                        fileItem.Status = Status.Existing;
                        AllFiles.Add(fileItem);
                    }

                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                fileItem.Status = Status.New;
                AllFiles.Add(fileItem);
            }
        }
        public string FileCount(ResourceType resourceType)
        {
            var resourceFiles = AllFiles.Where(f => f.ResourceType == resourceType);
            var existingsFiles = resourceFiles.Where(f => f.Status != Status.Existing);

            return existingsFiles.Count().ToString() + " / " + resourceFiles.Count().ToString();
        }


        public void Update()
        {
            if (isProcessRunning)
            {
                MessageBox.Show("File update process is already running.");
                return;
            }

            // TODO:
            //ProgressFrm progressDialog = new ProgressFrm() { Text = "Updating Packages..." };
            //progressDialog.TopMost = true;

            //update the file count for progress bar  
            var filesToSync = AllFiles.Where(f => f.Status != Status.Existing).ToList();
            //progressDialog.SetMaximum(filesToSync.Count());


            Thread backgroundThread = new(
                   new ThreadStart(() =>
                   {
                       isProcessRunning = true;
                       int counter = 0;

                       foreach (FileItem item in filesToSync)
                       {
                           counter++;

                           //check to see if folder exists
                           FileInfo file = new FileInfo(item.DestinFile);
                           if (!Directory.Exists(file.DirectoryName))
                           {
                               Directory.CreateDirectory(file.DirectoryName);
                           }
                           item.SourceFile.CopyTo(item.DestinFile, true);
                           item.Status = Status.Existing;


                           try
                           {
                               //progressDialog.SetInfo(item.DestinFile, counter, filesToSync.Count);
                               //progressDialog.UpdateProgress(); // Update progress in progressDialog
                           }
                           catch
                           {

                           }

                       }

                       // No need to reset the progress since we are closing the dialog
                       //progressDialog.BeginInvoke(new Action(() => progressDialog.Close()));

                       isProcessRunning = false;
                   }
                ));

            backgroundThread.Start();
            //progressDialog.ShowDialog();

            //
        }

        internal static void PurgeActivePackages(string appDataFolder)
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

        internal static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    //skip
                }
            }
        }

        internal static void CopyPackage(Package pkg)
        {
            var src = Path.Combine(UserInfo.LocalAppDataFolder("Packages"), pkg.PackageName);

            CopyFolder(src, pkg.PackageDestPath, true);
        }

        internal static bool CopyFolder(string sourceDirName, string destDirName, bool CopySubDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                return false;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            //if doesn't exists, create it
            Directory.CreateDirectory(destDirName);

            //copy files/folders to new location
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            //if copying sub folders
            if (CopySubDir)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    CopyFolder(subdir.FullName, tempPath, CopySubDir);
                }
            }

            return true;
        }


        /// <summary>
        /// Update the plotter registry file with the values specified through the Loader, then import it into the application profile registry entries.
        /// </summary>
        /// <param name="selectedOffice">Office that is currently selected in the working office combobox.</param>
        /// <param name="runVersion"></param>
        /// <param name="app"></param>
        /// <returns>Civil 3D units selected by the user (if the software has never previously been run). Otherwise, empty string.</returns>
        internal static string UpdateRegistryFile(Office selectedOffice, AppVersion runVersion, string app)
        {
            //get registry template file
            string srcPath = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), "Plotter.txt");

            //get destination path   
            string dstPath = Path.Combine(UserInfo.LocalAppDataFolder("Settings"), "Plotter.reg");

            File.Copy(srcPath, dstPath, true);

            //get the active profile
            var CurrentProfileName = RegistryInfo.CurrentProfileName(runVersion.Id, runVersion.Code);

            // TODO:!
            if (string.IsNullOrEmpty(CurrentProfileName) && app == "Civil3d")
            {
                // First time launch for the selected Civil 3D product - deal with unit selection here

                //UnitSelectorFrm unitSelectorFrm = new UnitSelectorFrm();
                //unitSelectorFrm.ShowDialog();

                //return unitSelectorFrm.Units;

            }
            else
            {
                string content = File.ReadAllText(dstPath);

                content = content.Replace("[version]", "acad" + runVersion.GetPlotterVersionOrDefault());
                content = content.Replace("[region]", selectedOffice.Region);
                content = content.Replace("[office]", selectedOffice.GetNetworkDriveOfficeNameOrDefault());
                content = content.Replace("[RegKey]", runVersion.RegKey);
                content = content.Replace("[profile]", CurrentProfileName);

                using (StreamWriter writer = new StreamWriter(dstPath))
                {
                    writer.Write(content);
                    writer.Close();
                }
                //dstPath = @"C:\Temp\Plotter.reg";
                //import the registry into the profile
                RegistryInfo.ImportRegistry(dstPath);
                return string.Empty;
            }

            return string.Empty;
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
                        CopyFolder(folder, localDataFolder, true);
                    }
                }
                else
                {
                    foreach (var folder in Directory.GetDirectories(srcProgramData))
                    {

                        var directoryInfo = new DirectoryInfo(folder);
                        var localDataFolder = userLocalAppData + "\\" + directoryInfo.Name;

                        if (Directory.GetFiles(localDataFolder).Length == 0 && Directory.GetDirectories(localDataFolder).Length == 0)
                            CopyFolder(folder, localDataFolder, true);
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
