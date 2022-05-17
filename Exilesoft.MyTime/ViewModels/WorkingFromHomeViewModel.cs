using Exilesoft.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Exilesoft.MyTime.ViewModels
{
    public class WorkingFromHomeViewModel
    {
        public SelectList EmployeeList { get; set; }
        public SelectList SupervisorList { get; set; }
        public IList<WorkingFromHomeRowViewModel> WorkingFromHomeRowList { get; set; }
        public string Description { get; set; }
        //public UpdateAttendanceViewModel AttendanceRecord { get; set; }
        public bool IsUpdate { get; set; }
        public double UpdateRecordId { get; set; }
        public int SupervisorId { get; set; }
        public string AttendanceDateIn { get; set; }
        public string AttendanceDateOut { get; set; }
        public IList<WorkingFromHomeTask> TaskList { get; set; }
        public IList<PendingAttendance> WorkingAttendenceList { get; set; }
    }
}