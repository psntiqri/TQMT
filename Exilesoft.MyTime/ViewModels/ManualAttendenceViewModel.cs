using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.MyTime.ViewModels
{
    public class ManualAttendenceViewModel
    {
        public int EmployeeId { get; set; }
        public int LocationId { get; set; }
        public string InOutMode { get; set; }
        public string AttendanceDate { get; set; }
    }
}