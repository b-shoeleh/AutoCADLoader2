using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADLoader.Models
{
    public class RegistryInfo
    {

        RegistryKey ProfileRegistryKey { get; set; }
        string KeyLocation { get; set; }
        public bool IsLoaded { get; set; }
        //public string ProductVersion { get; set; }

        //MAIN AREA OF REGISTRY
        // Computer\HKEY_CURRENT_USER\SOFTWARE\IBI Group\AcadLoader\
        public string Title { get; set; }
        public string Version { get; set; }
        public string Plugin { get; set; }




        //App specific registry
        // Computer\HKEY_CURRENT_USER\SOFTWARE\IBI Group\AcadLoader\Title_Version_Plugin
        public List<string> Packages { get; set; }

        /// <summary>
        /// Office code for the working office saved to registry when the user last started an application
        /// </summary>
        public string ActiveOffice { get; set; }

        //public string HomeRegion { get; set; }
        public string ActiveRegion { get; set; }
        public string Hardware { get; set; }



        public RegistryInfo(string officeCode)
        {
            ProfileRegistryKey = Registry.CurrentUser;
            KeyLocation = "IBI Group\\AcadLoader";
            Packages = new List<string>();
            IsLoaded = false;
            ActiveOffice = officeCode;// UserInfo.OfficeCode();
            LoadBaseRegistry();
        }

        /// <returns>Name of the registry subkey where Loader values for the most recently launched CAD application are - e.g. "AutoCAD_2022_".</returns>
        public string GetAppEntryLocation()
        {
            return $"{Title}_{Version}_{Plugin}";
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
        private void DeleteSubKeys(RegistryKey Root, string searchKey)
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


        public void DeleteProfile(string path, string regkey)
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
        /// <returns>Name of the profile most recently used within the specified Autodesk product, otherwise empty string.</returns>
        public static string CurrentProfileName(string versionNo, string versionCode)
        {
            string regPath = GetProfileKeyLocation(versionNo, versionCode);
            try
            {
                RegistryKey RKCU = Registry.CurrentUser.OpenSubKey(regPath, false);

                if (RKCU == null)
                {
                    return "";
                }
                else
                {
                    // The default (null) key contains the name of the most recently used profile within the Autodesk product.
                    return RKCU.GetValue(null).ToString();
                }
            }
            catch
            {
                return "";

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
        public void SaveUserRegistry()
        {
            RegistryKey RKCU = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey SubRKCU = RKCU.OpenSubKey(KeyLocation, true);

            if (SubRKCU == null)
            {

                SubRKCU = RKCU.CreateSubKey(KeyLocation);
            }

            //populate the app key infomration
            try
            {
                SubRKCU.SetValue("Title", Title);
                SubRKCU.SetValue("Version", Version);
                SubRKCU.SetValue("Plugin", Plugin);

            }
            catch
            {

            }


            //update the version key info
            string VersionKeyLocation = $@"{KeyLocation}\{GetAppEntryLocation()}";
            SubRKCU = RKCU.OpenSubKey(VersionKeyLocation, true);

            if (SubRKCU == null)
            {

                SubRKCU = RKCU.CreateSubKey(VersionKeyLocation);
            }


            try
            {
                //update the key values or create them if needed

                string packageList = string.Join(",", Packages);
                SubRKCU.SetValue("Packages", packageList);

                SubRKCU.SetValue("ActiveOffice", ActiveOffice);

                SubRKCU.SetValue("Hardware", Hardware);


            }
            catch
            {

            }
        }



        //public void SetHardware(string versionNo, string versionCode, bool useHardware)
        //{
        //    string keyLocation = "Software\\Autodesk\\AutoCAD\\" + versionNo + "\\" + versionCode + "\\3DGS Configuration\\Certification\\";

        //    RegistryKey RKCU = Registry.CurrentUser.OpenSubKey(keyLocation, true);

        //    if (RKCU == null)
        //    {
        //        //try
        //        //{
        //        //    RKCU = Registry.CurrentUser.OpenSubKey("Software\\Autodesk\\AutoCAD\\" + versionNo + "\\" + versionCode + "\\3DGS Configuration\\", true);
        //        //    RKCU = RKCU.CreateSubKey("Certification");
        //        //    RKCU = Registry.CurrentUser.OpenSubKey(keyLocation, true);

        //        //}
        //        //catch (Exception e)
        //        //{
        //        //    MessageBox.Show(e.Message);
        //        //}
        //    }
        //    else
        //    {

        //        //populate the app key infomration
        //        try
        //        {
        //            //new hex value
        //            //var hexValue = ;Convert.ToInt32("100", Convert.ToInt32("100", useHardware == true ? 1 : 0)

        //            RKCU.SetValue("HardwareEnabled", useHardware == true ? 1 : 0);
        //            // RKCU.SetValue("FeatureModeUsed", useHardware == true ? 1 : 0);

        //        }
        //        catch
        //        {

        //        }
        //    }

        //}

        public void LoadBaseRegistry()
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

        public void LoadVersionRegistry(string title, string version, string plugin)
        {
            RegistryKey RKCU = Registry.CurrentUser.OpenSubKey("Software", true);

            string VersionKeyLocation = $@"IBI Group\AcadLoader\{title}_{version}_{plugin}";
            RegistryKey SubRKCU = RKCU.OpenSubKey(VersionKeyLocation, true);

            IsLoaded = false;

            if (SubRKCU != null)
            {
                try
                {

                    ActiveOffice = SubRKCU.GetValue("ActiveOffice").ToString();
                    var _packages = SubRKCU.GetValue("Packages").ToString();
                    Packages.Clear();
                    if (!string.IsNullOrEmpty(_packages))
                    {
                        Packages = SubRKCU.GetValue("Packages").ToString().Split(',').ToList();
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
    }
}
