using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class AttendanceDataViewModel
    {       
        public string CardNumber { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string ActualHours { get; set; }
        public string OutOfOfficeHours { get; set; }
        public string LeaveType { get; set; }
        public string WorkeAtOffice { get; set; }
        public string WorkeAtHome { get; set; }
        public string Remarks { get; set; }

    }
}