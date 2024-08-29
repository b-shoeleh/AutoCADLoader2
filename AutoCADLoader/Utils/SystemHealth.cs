using System.IO;

namespace AutoCADLoader.Utils
{
    // TODO: Review this whole class/structure, lifted from previous project
    public class SystemHealth
    {
        int PB_WindowsTemp;
        int PB_UserTemp;
        int PB_HDD;


        public IEnumerable<Tuple<string, int>> Get()
        {
            IsTempFolderLarge();
            HDDSpace();

            Tuple<string, int>[] result = [
                new Tuple<string, int>("Windows temp", PB_WindowsTemp),
                new Tuple<string, int>("User temp", PB_UserTemp),
                new Tuple<string, int>("HDD space", PB_HDD)
            ];

            return result;
        }

        public void IsTempFolderLarge()
        {
            EventLogger.Log("Checking Temp Folder", System.Diagnostics.EventLogEntryType.Information);

            double FolderSize;
            double percentage;
            var maxTemp = 1000000000;

            //windows temp folder
            try
            {
                FolderSize = Utils.GetWinTempFolderSize();
                percentage = (FolderSize / maxTemp) * 100;
                PB_WindowsTemp = percentage > 100 ? 100 : (int)percentage;
            }
            catch
            {
                PB_WindowsTemp = 0;
            }

            //user temp folder
            try
            {
                FolderSize = Utils.GetUserTempFolderSize();

                percentage = (FolderSize / maxTemp) * 100;
                PB_UserTemp = percentage > 100 ? 100 : (int)percentage;
            }
            catch
            {
                PB_UserTemp = 0;
            }

            EventLogger.Log("Temp Folder size verified", System.Diagnostics.EventLogEntryType.Information);
        }

        public void HDDSpace()
        {
            EventLogger.Log("Checking Drive Space", System.Diagnostics.EventLogEntryType.Information);

            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    if (d.Name == "C:\\")
                    {
                        var usedSpace = ((double)d.TotalSize - d.TotalFreeSpace);
                        PB_HDD = (int)((usedSpace / d.TotalSize) * 100);
                    }
                }
            }

            EventLogger.Log("Drive Space verified", System.Diagnostics.EventLogEntryType.Information);
        }
    }
}
