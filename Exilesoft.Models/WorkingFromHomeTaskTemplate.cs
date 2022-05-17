using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exilesoft.Models
{
    public class WorkingFromHomeTaskTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int EmployeeId { get; set; }
        public int SupervisorId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Description { get; set; }
        public string TaskList { get; set; }
        public bool IsEnable { get; set; }
        public int PendingAttendanceId { get; set; }
    }
}
