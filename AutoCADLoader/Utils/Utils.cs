using System.Diagnostics;
using System.IO;
using System.Windows;

namespace AutoCADLoader.Utils
{
    public static class Utils
    {

        public static void startExplorer(string path)
        {
            //check path, if exists, open it.
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show("Path does not exist!");
            }

        }

        public static DirectoryInfo GetUserTempFolder()
        {
            var UserTempPath = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"temp"));
            return UserTempPath;
        }
        public static DirectoryInfo GetWinTempFolder()
        {
            var UserTempPath = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"temp"));
            return UserTempPath;
        }
        public static DirectoryInfo GetSystemDrive()
        {
            var systemDrive = new DirectoryInfo(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)));
            return systemDrive;
        }



        //command to open the folder
        public static void OpenUserTempFolder()
        {
            Utils.startExplorer(GetUserTempFolder().ToString());
        }
        public static void OpenWinTempFolder()
        {
            Utils.startExplorer(GetWinTempFolder().ToString());
        }
        public static void OpenSystemDrive()
        {
            Utils.startExplorer(GetSystemDrive().ToString());
        }


        public static double GetDirectorySize(DirectoryInfo d)
        {
            try
            {

                double size = 0;
                // Add file sizes.
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }
                // Add subdirectory sizes.
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += GetDirectorySize(di);
                }
                return size;
            }
            catch
            {
                return 0;
            }
        }

        public static double GetUserTempFolderSize()
        {
            return GetDirectorySize(GetUserTempFolder());
        }
        public static double GetWinTempFolderSize()
        {
            return GetDirectorySize(GetWinTempFolder());
        }

        public static double GetSystemDriveSize()
        {
            return GetDirectorySize(GetSystemDrive());
        }

        public static string ProgramDataLocation()
        {
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\{Properties.Settings.Default["CompanyFolder"].ToString()}\{Properties.Settings.Default["AppName"].ToString()}";
        }


        public static string SrcSettingsLocation(string fileName = "")
        {
            string path = Path.Combine(ProgramDataLocation(), "Settings");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                path = Path.Combine(path, fileName);
            }

            return path;
        }
    }
}
