using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;

namespace Exilesoft.MyTime.ViewModels
{
    public class AttendanceListViewModel
    {
        public AttendanceListViewModel()
        {
            this.EmployeeLocationList = new List<EmployeeStatistics>();
            this.AttendanceListEntryList = new List<AttendanceListEntry>();
        }

        public string EmployeeLocationContent { get; set; }
        public string AttendanceListEntryContent { get; set; }

        public List<EmployeeStatistics> EmployeeLocationList { get; set; }
        public List<AttendanceListEntry> AttendanceListEntryList { get; set; }
    }

    public class AttendanceListEntry
    {
        public string EmployeeName { get; set; }
        public string EntryTime { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
    }
}