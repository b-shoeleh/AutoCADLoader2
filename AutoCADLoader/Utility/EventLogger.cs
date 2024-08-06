using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADLoader.Utility
{
    public static class EventLogger
    {
        // new-eventlog -source "AutoCAD Loader" -logname "Arcadis-IBI" // This is done via the deployment script, but leaving here for reference
        private static bool _logInfo = false;
        private static string _eventLogName = "Arcadis-IBI";
        private static string _eventLogSourceName = "AutoCAD Loader";

        internal static void Initialize(bool logInfo)
        {
            _logInfo = logInfo;

#if DEBUG
            _logInfo = true;
#endif
        }


        public static void Log(string message, EventLogEntryType entryType)
        {
            if (_logInfo == false && entryType == EventLogEntryType.Information)
                return;

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
                using (EventLog eventLog = new EventLog("Application")) // Just log to the default place
                {
                    eventLog.Source = ".NET Runtime";
                    eventLog.WriteEntry(message, entryType, 1000);
                }
            }
        }


    }
}
