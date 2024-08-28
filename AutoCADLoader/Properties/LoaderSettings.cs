using System.IO;

namespace AutoCADLoader.Properties
{
    public static class LoaderSettings
    {
        public static string CompanyFolder { get; } = "Arcadis";
        public static string ApplicationName { get; } = "AutoCAD Loader";

        private static string _centralFolder { get; } = @"I:";
        private static string _centralLoaderSubfolder = @"_Arcadis\AutoCAD";


        public static string GetCentralFolderPath(string subfolder = "")
        {
            string path = Path.Combine(_centralFolder, _centralLoaderSubfolder);

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                return Path.Combine(path, subfolder);
            }

            return path;
        }

        public static string GetLocalCommonFolderPath(string subfolder = "")
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), CompanyFolder, ApplicationName);

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                path = Path.Combine(path, subfolder);
            }

            return path;
        }

        public static string GetLocalUserFolderPath(string subfolder = "")
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CompanyFolder, ApplicationName);

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                path = Path.Combine(path, subfolder);
            }

            return path;
        }
    }
}
