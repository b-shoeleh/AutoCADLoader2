using AutoCADLoader.Models.Offices;
using AutoCADLoader.Models.Packages;
using AutoCADLoader.Utility;
using AutoCADLoader.ViewModels;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;

namespace AutoCADLoader.Models
{
    // TODO: Might be better if this was DI instead of static, but this class may be removed eventually anyway
    public static class RegistryInfo
    {
        static RegistryKey ProfileRegistryKey { get; set; }
        static string KeyLocation { get; set; }
        public static bool IsLoaded { get; set; }
        //public string ProductVersion { get; set; }

        public static string Title { get; set; }
        public static string Version { get; set; }
        public static string Plugin { get; set; }




        //App specific registry
        // Computer\HKEY_CURRENT_USER\SOFTWARE\IBI Group\AcadLoader\Title_Version_Plugin
        public static List<string> Bundles { get; set; }

        /// <summary>
        /// Office code for the working office saved to registry when the user last started an application
        /// </summary>
        public static string ActiveOffice { get; set; }

        public static string ActiveRegion { get; set; }
        public static string Hardware { get; set; }


        static RegistryInfo()
        {
            ProfileRegistryKey = Registry.CurrentUser;
            KeyLocation = "Arcadis\\AutoCAD Loader";
            Bundles = [];
            IsLoaded = false;
            ActiveOffice = UserInfo.SavedOffice.OfficeCode;
            LoadBaseRegistry();
        }

        /// <summary>
        /// Dummy method to force constructor call.
        /// </summary>
        public static void Initialize()
        {

        }

        /// <returns>Name of the registry subkey where Loader values for the most recently launched CAD application are - e.g. "AutoCAD_2022".</returns>
        public static string GetAppEntryLocation()
        {
            string regPath = $"{Title}_{Version}";

            if(!string.IsNullOrWhiteSpace(Plugin))
            {
                regPath = $"{regPath}_{Plugin}";
            }

            return regPath;
        }


        //public void DeleteKey(string regkey)
        //{

        //    DeleteSubKeys(ProfileRegistryKey, regkey);
        //}

        /// <summary>
        /// delete the registry sub key from windows registry
        /// </summary>
        /// <param name="Root"></param>
        /// <param name="searchKey"></param>
        private static void DeleteSubKeys(RegistryKey Root, string searchKey)
        {

            if (Root == null)
            {
                return;
            }

            foreach (string keyname in Root.GetSubKeyNames())
            {
                try
                {
                    using (RegistryKey key = Root.OpenSubKey(keyname, true))
                    {
                        if (key == null)
                        {
                            return;
                        }

                        if (keyname == searchKey)
                        {
                            try
                            {

                                Root.DeleteSubKeyTree(searchKey);
                                break;

                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        DeleteSubKeys(key, searchKey);
                    }
                }
                catch (System.Security.SecurityException se)
                {

                }
                catch (Exception ex)
                {

                }
            }
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////


        public static void DeleteProfile(string path, string regkey)
        {
            RegistryKey key = Registry.CurrentUser;
            //open subkey
            RegistryKey profile = key.OpenSubKey(path, true);

            if (profile != null)
            {
                try
                {
                    profile.DeleteSubKeyTree(regkey);
                    profile.Close();

                }
                catch
                {
                    //doesn't exist
                }
            }
        }


        public static void ImportRegistry(string filePath)
        {
            Process proc = new Process();
            try
            {
                proc.StartInfo.FileName = "reg.exe";
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;

                string command = "import \"" + filePath + "\"";
                proc.StartInfo.Arguments = command;
                proc.Start();

                proc.WaitForExit();
            }
            catch (System.Exception)
            {
                proc.Dispose();
            }
        }

        /// <param name="versionNo">Autodesk product version number (ACADVER), e.g. "R24.1"</param>
        /// <param name="versionCode">Autodesk product version code, e.g. "ACAD-5101:409"</param>
        /// <returns>The registry subkey location of the Autodesk profile for the Autodesk application specified - e.g. "Software\Autodesk\AutoCAD\R24.1\ACAD-5101:409\Profiles".</returns>
        public static string GetProfileKeyLocation(string versionNo, string versionCode)
        {
            return "Software\\Autodesk\\AutoCAD\\" + versionNo + "\\" + versionCode + "\\Profiles";

            //RegistryKey RKCU = Registry.CurrentUser.OpenSubKey(regPath, true);

            //foreach (string Keyname in RKCU.GetSubKeyNames())
            //{
            //    if (Keyname.Substring(0, 4).ToUpper() == "ACAD")
            //    {
            //        regPath += "\\" + Keyname + "\\Profiles";
            //    }
            //}

            //found the key, now look for profile and check what is the default
            //RKCU = Registry.CurrentUser.OpenSubKey(regPath, true);

            // return RKCU.Name;
            //return regPath;

        }


        /// <summary>
        /// looks at the root of that profiles and select what is current, the only key there.
        /// </summary>
        /// <param name="versionNo">Autodesk product version number (ACADVER), e.g. "R24.1"</param>
        /// <param name="versionCode">Autodesk product version code, e.g. "ACAD-5101:409"</param>
        /// <returns>Name of the profile most recently used within the specified Autodesk product, otherwise null.</returns>
        public static string? CurrentProfileName(string versionNo, string versionCode)
        {
            string regPath = GetProfileKeyLocation(versionNo, versionCode);
            try
            {
                RegistryKey? RKCU = Registry.CurrentUser.OpenSubKey(regPath, false);

                // The default (null) key contains the name of the most recently used profile within the Autodesk product
                return RKCU?.GetValue(null) as string;
            }
            catch
            {
                return null;
            }
        }
        //public void UpdateToolbarFile(string filePath, string versionNo, string versionCode)
        //{
        //    //get the path of registry for version specified
        //    string regPath = GetProfileKeyLocation(versionNo, versionCode);
        //    string sourceFile = filePath + "\\Toolbars." + versionNo + ".reg";
        //    string destinFile = filePath + "\\ToolbarsImport.reg";

        //    try
        //    {
        //        RegistryKey RKCU = Registry.CurrentUser.OpenSubKey(regPath, true);

        //        if (File.Exists(sourceFile))
        //        {

        //            int line_to_edit = 3; // Warning: 1-based indexing!


        //            System.IO.StreamReader file = new System.IO.StreamReader(sourceFile);

        //            StreamWriter writer = new StreamWriter(destinFile);


        //            string line;
        //            int counter = 0;
        //            while ((line = file.ReadLine()) != null)
        //            {
        //                counter++;
        //                if (counter == line_to_edit)
        //                {
        //                    //edit the line
        //                    line = "[" + RKCU.Name + "\\" + Profile + "\\Toolbars]";

        //                }

        //                writer.WriteLine(line);


        //            }

        //            writer.Close();

        //            //import the key
        //            ImportRegistry(destinFile);
        //        }
        //    }
        //    catch
        //    {
        //        //could not save user profile
        //    }



        //}

        //public void SaveUserToolBars(string versionNo, string versionCode, string filePath)
        //{
        //    //get the path of registry for version specified
        //    string regPath = GetProfileKeyLocation(versionNo, versionCode);

        //    try
        //    {
        //        //found the key, now look for profile and check what is the default
        //        RegistryKey RKCU = Registry.CurrentUser.OpenSubKey(regPath, true);

        //        if (RKCU != null)
        //        {
        //            string defaultProfile = RKCU.GetValue(null).ToString();

        //            regPath += "\\" + defaultProfile + "\\Toolbars";
        //            //now get the toolbars of the default profile
        //            RKCU = Registry.CurrentUser.OpenSubKey(regPath, true);

        //            //export the profile to user's profile
        //            string fileName = filePath + "\\Toolbars." + versionNo + ".reg";
        //            if (File.Exists(fileName))
        //            {

        //                string buFileName = filePath + "\\Toolbars." + versionNo + "old" + ".reg";
        //                //rename the file
        //                File.Move(fileName, buFileName);
        //            }

        //            if (!File.Exists(fileName))
        //            {
        //                exportRegistry(RKCU.Name, fileName);
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        //could not find registry
        //    }
        //}

        //public string GetUserWorkspace(string versionNo, string versionCode)
        //{
        //    string wspace = "";
        //    //get the path of registry for version specified
        //    string regPath = GetProfileKeyLocation(versionNo, versionCode);

        //    try
        //    {
        //        //found the key, now look for profile and check what is the default
        //        RegistryKey RKCU = Registry.CurrentUser.OpenSubKey(regPath, true);

        //        if (RKCU != null)
        //        {
        //            string defaultProfile = RKCU.GetValue(null).ToString();

        //            regPath += "\\" + defaultProfile + "\\General";
        //            //now get the toolbars of the default profile
        //            RKCU = Registry.CurrentUser.OpenSubKey(regPath, false);
        //            if (RKCU != null)
        //            {
        //                wspace = RKCU.GetValue("WSCURRENT").ToString();
        //            }

        //        }
        //    }
        //    catch
        //    {
        //        //could not find registry
        //    }

        //    return wspace;
        //}

        //void exportRegistry(string strKey, string filepath)
        //{
        //    try
        //    {
        //        using (Process proc = new Process())
        //        {
        //            proc.StartInfo.FileName = "reg.exe";
        //            proc.StartInfo.UseShellExecute = false;
        //            proc.StartInfo.RedirectStandardOutput = true;
        //            proc.StartInfo.RedirectStandardError = true;
        //            proc.StartInfo.CreateNoWindow = true;
        //            proc.StartInfo.Arguments = "export \"" + strKey + "\" \"" + filepath + "\" /y";
        //            proc.Start();
        //            string stdout = proc.StandardOutput.ReadToEnd();
        //            string stderr = proc.StandardError.ReadToEnd();
        //            proc.WaitForExit();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // handle exception
        //    }


        //}

        /// <summary>
        /// Update the registry values for AutoCAD Loader (to remember settings on next time the Loader is launched).
        /// </summary>
        public static void SaveUserRegistry()
        {
            RegistryKey? RKCU = Registry.CurrentUser.OpenSubKey("Software", true);

            if (RKCU is null)
            {
                EventLogger.Log("Error saving user registry settings - HCKU Software key does not exist.", EventLogEntryType.Error);
                MessageBox.Show("Error saving user settings.");
                return;
            }

            RegistryKey? SubRKCU = RKCU.OpenSubKey(KeyLocation, true);
            SubRKCU ??= RKCU.CreateSubKey(KeyLocation);

            try
            {
                SubRKCU.SetValue("Title", Title);
                SubRKCU.SetValue("Version", Version);
                SubRKCU.SetValue("Plugin", Plugin);
            }
            catch
            {
                EventLogger.Log("Error saving user registry settings - cannot set values.", EventLogEntryType.Error);
                MessageBox.Show("Error saving user settings.");
            }

            //update the version key info
            string VersionKeyLocation = $@"{KeyLocation}\{GetAppEntryLocation()}";
            SubRKCU = RKCU.OpenSubKey(VersionKeyLocation, true);
            SubRKCU ??= RKCU.CreateSubKey(VersionKeyLocation);

            try
            {
                //update the key values or create them if needed
                string packageList = string.Join(",", Bundles);
                packageList ??= string.Empty;

                SubRKCU.SetValue("Packages", packageList);
                SubRKCU.SetValue("ActiveOffice", ActiveOffice);
                SubRKCU.SetValue("Hardware", Hardware);
            }
            catch
            {
                EventLogger.Log("Error saving user registry settings - cannot set values.", EventLogEntryType.Error);
                MessageBox.Show("Error saving user settings.");
            }
        }

        public static void UpdateUserRegistry(
            string title,
            string version,
            string? plugin,
            IEnumerable<Bundle>? bundles,
            string activeOffice,
            bool hardwareAccelerationEnabled)
        {
            Title = title;
            Version = version;
            Plugin = plugin ?? string.Empty;
            Bundles = [.. bundles?.Select(b => b.Title)];
            ActiveOffice = activeOffice;
            Hardware = hardwareAccelerationEnabled ? "true" : "false";

            SaveUserRegistry();
        }

        public static void LoadBaseRegistry()
        {
            RegistryKey RKCU = Registry.CurrentUser.OpenSubKey("Software", true);

            //get the last used autocad version
            KeyLocation = "IBI Group\\AcadLoader\\";

            try
            {
                RegistryKey SubRKCU = RKCU.OpenSubKey(KeyLocation, true);

                if (SubRKCU != null)
                {
                    try
                    {

                        //ProductVersion = SubRKCU.GetValue("ProductVersion").ToString();
                        Title = SubRKCU.GetValue("Title").ToString();
                        Version = SubRKCU.GetValue("Version").ToString();
                        Plugin = SubRKCU.GetValue("Plugin").ToString();
                    }
                    catch { }
                }


                //if (ProductVersion != Application.ProductVersion)
                //{
                //    DeleteKey("AcadLoader");
                //}
                //else
                //{
                LoadVersionRegistry(Title, Version, Plugin);
                //}
            }
            catch
            {

            }
        }

        public static void LoadVersionRegistry(string title, string version, string plugin)
        {
            RegistryKey RKCU = Registry.CurrentUser.OpenSubKey("Software", true);

            string VersionKeyLocation = $@"IBI Group\AcadLoader\{title}_{version}_{plugin}";
            RegistryKey SubRKCU = RKCU.OpenSubKey(VersionKeyLocation, true);

            IsLoaded = false;

            // TODO: Review this, may need improvement
            if (SubRKCU != null)
            {
                try
                {

                    ActiveOffice = SubRKCU.GetValue("ActiveOffice").ToString();
                    var _packages = SubRKCU.GetValue("Packages").ToString();
                    Bundles.Clear();
                    if (!string.IsNullOrEmpty(_packages))
                    {
                        Bundles = SubRKCU.GetValue("Packages").ToString().Split(',').ToList();
                    }


                    try
                    {
                        Hardware = SubRKCU.GetValue("Hardware").ToString();
                    }
                    catch
                    {
                        Hardware = "False";
                    }

                    IsLoaded = true;

                }
                catch

                {
                    IsLoaded = false;
                }
            }


            if (IsLoaded == false)
            {

                ActiveOffice = null;// ActiveOffice;// UserInfo.OfficeCode();
                IsLoaded = false;
                Hardware = "true";

            }

        }

        public static bool KeyExists(string key)
        {
            string regPath = "Software\\" + key;

            try
            {
                RegistryKey RKCU = Registry.LocalMachine.OpenSubKey(regPath, false);

                if (RKCU != null)
                {
                    //key found, so installed
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                return false;

            }

        }

        /// <summary>
        /// Update the plotter file for registry import
        /// </summary>
        /// <returns>Civil 3D unit selection (if found)</returns>
        public static string? UpdateProfilePlotters(InstalledAutodeskApplication selectedApplication, Office selectedOffice)
        {
            string units = FileUpdaterLoader.UpdateRegistryFile(selectedOffice, selectedApplication.AppVersion, selectedApplication.AutodeskApplication.Title);
            return units;
        }

        // TODO: Consider moving this out into an application class
        public static void InjectSupportPaths(string versionId, string profileName)
        {
            string registryPath = @$"HKEY_CURRENT_USER\SOFTWARE\{versionId}\Profiles\{profileName}\General";
            string registryValueName = "ACAD";

            // Support path types to add
            string[] pathTypesToAdd = ["Fonts", "Pats"];

            // Get the current support paths within the current AutoCAD profile
            string? supportPaths = RegistryFunctions.GetStringWithOptions(registryValueName, registryPath, RegistryValueOptions.DoNotExpandEnvironmentNames);

            // Check if the support path already exists, and if not add it
            if (!string.IsNullOrWhiteSpace(supportPaths))
            {
                foreach (string pathType in pathTypesToAdd)
                {
                    string expandedPath = UserInfo.LocalAppDataFolder(pathType);
                    string nonExpandedPath = UserInfo.LocalAppDataFolder(pathType, false);
                    // AutoCAD will re-save non-expanded paths to this format, so this is what needs to be searched for to ensure no duplicates
                    nonExpandedPath = nonExpandedPath.Replace("%localappdata%", @"%UserProfile%\AppData\Local", StringComparison.InvariantCultureIgnoreCase);

                    if (!supportPaths.Contains(expandedPath, StringComparison.InvariantCultureIgnoreCase) && !supportPaths.Contains(nonExpandedPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        supportPaths = string.Concat(supportPaths, nonExpandedPath, ";");
                    }
                }
            }
        }
    }
}
