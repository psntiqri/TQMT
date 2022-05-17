using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.TimeManager
{
    public class Logger
    {
        private static EventLog _myTimeEventLog;

        public static EventLog MyTimeEventLog
        {
            get { return _myTimeEventLog; }
            set { _myTimeEventLog = value; }
        }

        public static void Log(string text,EventLogEntryType eventLogEntryType)
        {
            _myTimeEventLog.WriteEntry(string.Format("MyTime synchronization service stoped at : {0}", System.DateTime.Now),
               EventLogEntryType.Information);
        }
       
    }
}
