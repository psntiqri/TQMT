using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class SyncSessionLog
    {
        public int Id { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public int NumberOfEmployeeRecords { get; set; }
        public int NumberOfVisitorRecords { get; set; }
        public int NuberOfExternalRecords { get; set; }
        //0 - not synced ; 1 - synced
        public int IsSynced { get; set; }
    }

    public enum SyncStatus
    {
        Successful, Failed, PartiallyCompleted
    }
}