using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Exilesoft.Models
{
    public class EmployeeOnSite
    {
        public int Id { get; set; }
        [ForeignKey("EmployeeId")]
        public EmployeeEnrollment EmployeeEnrollment { get; set; }
        public int EmployeeId { get; set; }
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }
        [ForeignKey("LocationId")]
        public Location Location { get; set; }
        public int? LocationId { get; set; }
        public string MobileNumber { get; set; }        
        public bool IsPermanant { get; set; }
    }
}