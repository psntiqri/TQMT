
using System;
using Exilesoft.Models;

namespace Exilesoft.MyTime.Common
{
    public class AttendanceView
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Time { get; set; }
        public DateTime Date { get; set; }
        public string InOutMode { get; set; }
        public int LocationId { get; set; }
        public int CardNo { get; set; }
        public EmployeeData Employee { get; set; }
    }
}