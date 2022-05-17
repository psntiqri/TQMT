using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class SyncServiceStatusInfo
    {
        public int Id { get; set; }
        public string ServiceStatus { get; set; }
        public string SynchronizationStatus { get; set; }
        public DateTime? LastServiceStartedAt { get; set; }
        public DateTime? LastServiceStoppedAt { get; set; }
        public DateTime? NextServiceScheduledAt { get; set; }
        public string CurrentSynchronizingMessage { get; set; }
        public int CurrentSynchronizingLocationID { get; set; }
    }

    public enum SyncServiceStatus
    {
        Running, Stopped
    }

    public enum SyncProcessStatus
    {
        Synchronizing, Idle
    }
}