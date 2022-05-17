using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Exilesoft.Models;

namespace Exilesoft.Models
{
    public class Leave
    {
        public EmployeeEnrollment Employee { get; set; }
        public DateTime Date { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string LeaveDeductedFrom { get; set; }
    }

    public enum LeaveType
    {
        FullDay, HalfDay
    }

    public enum MyLeaveType
    {
        HALFDAY_NO, HALFDAY_AM, HALFDAY_PM
    }
}