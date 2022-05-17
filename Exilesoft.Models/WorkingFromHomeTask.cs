using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class WorkingFromHomeTask
    {
        public int Id { get; set; }
        public int PendingAttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
    }
}
