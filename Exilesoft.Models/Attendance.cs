
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        [ForeignKey("EmployeeId")]
        public EmployeeEnrollment Employee { get; set; }
        public int EmployeeId { get; set; }
        public int CardNo { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int VerifyMode { get; set; }
        public string InOutMode { get; set; }
        public int WorkCode { get; set; }
        [ForeignKey("LocationId")]
        public Location Location { get; set; }
        public int LocationId { get; set; }
    }
}