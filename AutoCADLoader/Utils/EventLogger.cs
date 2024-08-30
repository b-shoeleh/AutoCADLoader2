using System.Diagnostics;

namespace AutoCADLoader.Utils
{
    public static class EventLogger
    {
        // new-eventlog -source "AutoCAD Loader" -logname "Arcadis-IBI" // This is done via the deployment script, but leaving here for reference
        private static bool _logInfo = false;
        private static string _eventLogName = "Arcadis-IBI";
        private static string _eventLogSourceName = "AutoCAD Loader";

        public static void Initialize(bool logInfo)
        {
            _logInfo = logInfo;
        }


        public static void Log(string message, EventLogEntryType entryType)
        {
            if (!_logInfo && entryType == EventLogEntryType.Information)
            {
                return;
            }

            try
            {
                using (EventLog eventLog = new EventLog(_eventLogName))
                {
                    eventLog.Source = _eventLogSourceName;
                    eventLog.WriteEntry(message, entryType);
                }
            }
            catch
            {
                using (EventLog eventLog = new("Application")) // Just log to the default place
                {
                    eventLog.Source = ".NET Runtime";
                    eventLog.WriteEntry(message, entryType, 1000);
                }
            }
        }


    }
}
