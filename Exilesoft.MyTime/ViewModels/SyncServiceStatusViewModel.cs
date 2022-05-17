using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class SyncServiceStatusViewModel
    {
        public string ServiceStatusIndicator { get; set; }
        public string serviceStatus { get; set; }
        public string LastSyncStart { get; set; }
        public string LastSyncCompleted { get; set; }
        public string NextScheduleSync { get; set; }
        public string ProcessStatus { get; set; }
        public string Notes { get; set; }
        public string DeviceClearSettingTable { get; set; }
    }

    public class SessionLogSummeryViewModel 
    {
        public SessionLogSummeryViewModel()
        {
            this.SessionLogViewModelList = new List<SessionLogViewModel>();
        }

        public string ResultGraphData { get; set; }
        public System.Collections.Generic.IList<SessionLogViewModel> SessionLogViewModelList { get; set; }
    }

    public class SessionLogViewModel 
    {
        public int SessionID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Status { get; set; }
        public string NoOfEmployeeEntries { get; set; }
        public string NoOfVisitorEntrie { get; set; }
        public string StatusIcon { get; set; }
    }

    public class MachineLogViewModel
    {
        public int MachineLogID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string NoOfEmployeeEntries { get; set; }
        public string NoOfVisitorEntrie { get; set; }
        public string StatusIcon { get; set; }
    }

    public class SyncLogViewModel
    {
        public string StatusIcon { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
    }
}